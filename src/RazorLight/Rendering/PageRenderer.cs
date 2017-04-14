using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using RazorLight.Templating;
using RazorLight.Internal;

namespace RazorLight.Rendering
{
	public class PageRenderer : IDisposable
	{
		private readonly IViewBufferScope _bufferScope;
		private readonly HtmlEncoder _htmlEncoder;

		private readonly TemplatePage razorPage;
		private readonly IPageLookup pageLookup;

		public PageRenderer(TemplatePage page, IPageLookup pageLookup)
		{
			this.razorPage = page;
			this.pageLookup = pageLookup;

			_htmlEncoder = HtmlEncoder.Default;
			_bufferScope = new MemoryPoolViewBufferScope();
			ViewStartPages = new List<TemplatePage>();
			PreRenderCallbacks = new PreRenderActionList();
		}

		public List<TemplatePage> ViewStartPages { get; }
		public PreRenderActionList PreRenderCallbacks { get; }

		public virtual async Task RenderAsync(PageContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			ViewBufferTextWriter bodyWriter = await RenderPageAsync(this.razorPage, context, invokeViewStarts: true);
			await RenderLayoutAsync(context, bodyWriter);
		}

		private Task RenderPageCoreAsync(TemplatePage page, PageContext context)
		{
			page.PageContext = context;
			page.PageLookup = this.pageLookup;
			return page.ExecuteAsync();
		}

		private async Task<ViewBufferTextWriter> RenderPageAsync(
			TemplatePage page,
			PageContext context,
			bool invokeViewStarts)
		{
			var writer = context.Writer as ViewBufferTextWriter;
			if (writer == null)
			{
				Debug.Assert(_bufferScope != null);

				// If we get here, this is likely the top-level page (not a partial) - this means
				// that context.Writer is wrapping the output stream. We need to buffer, so create a buffered writer.
				ViewBuffer buffer = new ViewBuffer(_bufferScope, page.Path, ViewBuffer.ViewPageSize);
				writer = new ViewBufferTextWriter(buffer, context.Writer.Encoding, _htmlEncoder, context.Writer);
			}
			else
			{
				// This means we're writing something like a partial, where the output needs to be buffered.
				// Create a new buffer, but without the ability to flush.
				ViewBuffer buffer = new ViewBuffer(_bufferScope, page.Path, ViewBuffer.ViewPageSize);
				writer = new ViewBufferTextWriter(buffer, context.Writer.Encoding);
			}

			// The writer for the body is passed through the ViewContext, allowing things like HtmlHelpers
			// and ViewComponents to reference it.
			var oldWriter = context.Writer;
			var oldFilePath = context.ExecutingFilePath;

			context.Writer = writer;
			context.ExecutingFilePath = page.Path;

			try
			{
				//Apply page specific callbacks first
				ExecutePageCallbacks(page, context.PrerenderCallbacks);

				//Apply engine-global callbacks
				ExecutePageCallbacks(page, PreRenderCallbacks.ToList());

				if (invokeViewStarts)
				{
					// Execute view starts using the same context + writer as the page to render.
					await RenderViewStartsAsync(context);
				}

				await RenderPageCoreAsync(page, context);
				return writer;
			}
			finally
			{
				context.Writer = oldWriter;
				context.ExecutingFilePath = oldFilePath;
			}
		}

		private async Task RenderViewStartsAsync(PageContext context)
		{
			string layout = null;
			var oldFilePath = context.ExecutingFilePath;
			try
			{
				for (var i = 0; i < ViewStartPages.Count; i++)
				{
					var viewStart = ViewStartPages[i];
					context.ExecutingFilePath = viewStart.Path;

					// If non-null, copy the layout value from the previous view start to the current. Otherwise leave
					// Layout default alone.
					if (layout != null)
					{
						viewStart.Layout = layout;
					}

					await RenderPageCoreAsync(viewStart, context);

					// Pass correct absolute path to next layout or the entry page if this view start set Layout to a
					// relative path.
					layout = GetAbsolutePath(viewStart.Path, viewStart.Layout);
				}
			}
			finally
			{
				context.ExecutingFilePath = oldFilePath;
			}

			// If non-null, copy the layout value from the view start page(s) to the entry page.
			if (layout != null)
			{
				razorPage.Layout = layout;
			}
		}

		private async Task RenderLayoutAsync(
			PageContext context,
			ViewBufferTextWriter bodyWriter)
		{
			// A layout page can specify another layout page. We'll need to continue
			// looking for layout pages until they're no longer specified.
			var previousPage = razorPage;
			var renderedLayouts = new List<TemplatePage>();

			// This loop will execute Layout pages from the inside to the outside. With each
			// iteration, bodyWriter is replaced with the aggregate of all the "body" content
			// (including the layout page we just rendered).
			while (!string.IsNullOrEmpty(previousPage.Layout))
			{
				if (!bodyWriter.IsBuffering)
				{
					// Once a call to RazorPage.FlushAsync is made, we can no longer render Layout pages - content has
					// already been written to the client and the layout content would be appended rather than surround
					// the body content. Throwing this exception wouldn't return a 500 (since content has already been
					// written), but a diagnostic component should be able to capture it.

					throw new InvalidOperationException("Layout cannot be rendered");
				}

				TemplatePage layoutPage = GetLayoutPage(previousPage.Layout);

				if (renderedLayouts.Count > 0 &&
					renderedLayouts.Any(l => string.Equals(l.Path, layoutPage.Path, StringComparison.Ordinal)))
				{
					// If the layout has been previously rendered as part of this view, we're potentially in a layout
					// rendering cycle.
					throw new InvalidOperationException("Layout has circular reference");
				}

				// Notify the previous page that any writes that are performed on it are part of sections being written
				// in the layout.
				previousPage.IsLayoutBeingRendered = true;
				layoutPage.PreviousSectionWriters = previousPage.SectionWriters;
				layoutPage.BodyContent = bodyWriter.Buffer;
				bodyWriter = await RenderPageAsync(layoutPage, context, invokeViewStarts: false);

				renderedLayouts.Add(layoutPage);
				previousPage = layoutPage;
			}

			// Now we've reached and rendered the outer-most layout page. Nothing left to execute.

			// Ensure all defined sections were rendered or RenderBody was invoked for page without defined sections.
			foreach (var layoutPage in renderedLayouts)
			{
				layoutPage.EnsureRenderedBodyOrSections();
			}

			if (bodyWriter.IsBuffering)
			{
				// If IsBuffering - then we've got a bunch of content in the view buffer. How to best deal with it
				// really depends on whether or not we're writing directly to the output or if we're writing to
				// another buffer.
				var viewBufferTextWriter = context.Writer as ViewBufferTextWriter;
				if (viewBufferTextWriter == null || !viewBufferTextWriter.IsBuffering)
				{
					// This means we're writing to a 'real' writer, probably to the actual output stream.
					// We're using PagedBufferedTextWriter here to 'smooth' synchronous writes of IHtmlContent values.
					using (var writer = _bufferScope.CreateWriter(context.Writer))
					{
						await bodyWriter.Buffer.WriteToAsync(writer, _htmlEncoder);
					}
				}
				else
				{
					// This means we're writing to another buffer. Use MoveTo to combine them.
					bodyWriter.Buffer.MoveTo(viewBufferTextWriter.Buffer);
					return;
				}
			}
		}

		private TemplatePage GetLayoutPage(string layoutKey)
		{
			PageLookupResult layoutPageResult = pageLookup.GetPage(layoutKey);
			if (!layoutPageResult.Success)
			{
				throw new RazorLightException($"Layout cannot be located ({layoutKey})");
			}

			TemplatePage layoutPage = layoutPageResult.ViewEntry.PageFactory();

			return layoutPage;
		}

		private void ExecutePageCallbacks(TemplatePage page, ICollection<Action<TemplatePage>> callbacks)
		{
			if(callbacks?.Count > 0)
			{
				foreach (var callback in callbacks)
				{
					try
					{
						callback(page);
					}
					catch (Exception)
					{
						//Ignore
					}
				}
			}
		}

		public void Dispose()
		{
			((IDisposable)_bufferScope)?.Dispose();
		}

		#region Helpers

		public string GetAbsolutePath(string executingFilePath, string pagePath)
		{
			if (string.IsNullOrEmpty(pagePath))
			{
				// Path is not valid; no change required.
				return pagePath;
			}

			if (IsApplicationRelativePath(pagePath))
			{
				// An absolute path already; no change required.
				return pagePath;
			}

			if (!IsRelativePath(pagePath))
			{
				// A page name; no change required.
				return pagePath;
			}

			// Given a relative path i.e. not yet application-relative (starting with "~/" or "/"), interpret
			// path relative to currently-executing view, if any.
			if (string.IsNullOrEmpty(executingFilePath))
			{
				// Not yet executing a view. Start in app root.
				return "/" + pagePath;
			}

			// Get directory name (including final slash) but do not use Path.GetDirectoryName() to preserve path
			// normalization.
			var index = executingFilePath.LastIndexOf('/');
			Debug.Assert(index >= 0);
			return executingFilePath.Substring(0, index + 1) + pagePath;
		}

		private static bool IsApplicationRelativePath(string name)
		{
			Debug.Assert(!string.IsNullOrEmpty(name));
			return name[0] == '~' || name[0] == '/';
		}

		private static bool IsRelativePath(string name)
		{
			Debug.Assert(!string.IsNullOrEmpty(name));

			// Though ./ViewName looks like a relative path, framework searches for that view using view locations.
			return name.EndsWith("cs", StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}