using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using RazorLight.Internal.Buffering;

namespace RazorLight
{
	public class TemplateRenderer
	{
		private readonly HtmlEncoder _htmlEncoder;
		private readonly IEngineHandler _engineHandler;
		private readonly IViewBufferScope _bufferScope;

		public TemplateRenderer(
			IEngineHandler engineHandler,
			HtmlEncoder htmlEncoder,
			IViewBufferScope bufferScope)
		{
			_engineHandler = engineHandler ?? throw new ArgumentNullException(nameof(engineHandler));
			_bufferScope = bufferScope ?? throw new ArgumentNullException(nameof(bufferScope));
			_htmlEncoder = htmlEncoder ?? throw new ArgumentNullException(nameof(htmlEncoder));
		}

		///// <summary>
		///// Gets the sequence of _ViewStart <see cref="ITemplatePage"/> instances that are executed by this view.
		///// </summary>
		//public IReadOnlyList<ITemplatePage> ViewStartPages { get; }

		public virtual async Task RenderAsync(ITemplatePage page)
		{
			var context = page.PageContext;

			var bodyWriter = await RenderPageAsync(page, context, invokeViewStarts: false).ConfigureAwait(false);
			await RenderLayoutAsync(page, context, bodyWriter).ConfigureAwait(false);
		}

		private async Task<ViewBufferTextWriter> RenderPageAsync(
			ITemplatePage page,
			PageContext context,
			bool invokeViewStarts)
		{
			var writer = context.Writer as ViewBufferTextWriter;
			if (writer == null)
			{
				Debug.Assert(_bufferScope != null);

				// If we get here, this is likely the top-level page (not a partial) - this means
				// that context.Writer is wrapping the output stream. We need to buffer, so create a buffered writer.
				var buffer = new ViewBuffer(_bufferScope, page.Key, ViewBuffer.ViewPageSize);
				writer = new ViewBufferTextWriter(buffer, context.Writer.Encoding, _htmlEncoder, context.Writer);
			}
			else
			{
				// This means we're writing something like a partial, where the output needs to be buffered.
				// Create a new buffer, but without the ability to flush.
				var buffer = new ViewBuffer(_bufferScope, page.Key, ViewBuffer.ViewPageSize);
				writer = new ViewBufferTextWriter(buffer, context.Writer.Encoding);
			}

			// The writer for the body is passed through the PageContext, allowing things like HtmlHelpers
			// and ViewComponents to reference it.
			var oldWriter = context.Writer;
			var oldFilePath = context.ExecutingPageKey;

			context.Writer = writer;
			context.ExecutingPageKey = page.Key;

			try
			{
				//Apply engine-global callbacks
				ExecutePageCallbacks(page, _engineHandler.Options.PreRenderCallbacks.ToList());

				if (invokeViewStarts)
				{
					// Execute view starts using the same context + writer as the page to render.
					await RenderViewStartsAsync(context).ConfigureAwait(false);
				}

				await RenderPageCoreAsync(page, context).ConfigureAwait(false);
				return writer;
			}
			finally
			{
				context.Writer = oldWriter;
				context.ExecutingPageKey = oldFilePath;
			}
		}

		private async Task RenderPageCoreAsync(ITemplatePage page, PageContext context)
		{
			page.PageContext = context;
			page.IncludeFunc = async (key, model) =>
			{
				ITemplatePage template = await _engineHandler.CompileTemplateAsync(key);

				await _engineHandler.RenderIncludedTemplateAsync(template, model, context.Writer, context.ViewBag, this);
			};

			//_pageActivator.Activate(page, context);

			await page.ExecuteAsync().ConfigureAwait(false);
		}

		private Task RenderViewStartsAsync(PageContext context)
		{
			return Task.CompletedTask;

			//string layout = null;
			//string oldPageKey = context.ExecutingPageKey;
			//try
			//{
			//    for (var i = 0; i < ViewStartPages.Count; i++)
			//    {
			//        var viewStart = ViewStartPages[i];
			//        context.ExecutingPageKey = viewStart.Key;

			//        // If non-null, copy the layout value from the previous view start to the current. Otherwise leave
			//        // Layout default alone.
			//        if (layout != null)
			//        {
			//            viewStart.Layout = layout;
			//        }

			//        await RenderPageCoreAsync(viewStart, context);

			//        // Pass correct absolute path to next layout or the entry page if this view start set Layout to a
			//        // relative path.
			//        layout = _viewEngine.GetAbsolutePath(viewStart.Key, viewStart.Layout);
			//    }
			//}
			//finally
			//{
			//    context.ExecutingPageKey = oldPageKey;
			//}

			//// If non-null, copy the layout value from the view start page(s) to the entry page.
			//if (layout != null)
			//{
			//    RazorPage.Layout = layout;
			//}
		}

		private async Task RenderLayoutAsync(
			ITemplatePage page,
			PageContext context,
			ViewBufferTextWriter bodyWriter)
		{
			// A layout page can specify another layout page. We'll need to continue
			// looking for layout pages until they're no longer specified.
			var previousPage = page;
			var renderedLayouts = new List<ITemplatePage>();

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

					throw new InvalidOperationException("Layout can not be rendered");
				}

				ITemplatePage layoutPage = await _engineHandler.CompileTemplateAsync(previousPage.Layout).ConfigureAwait(false);
				layoutPage.SetModel(context.Model);

				if (renderedLayouts.Count > 0 &&
					renderedLayouts.Any(l => string.Equals(l.Key, layoutPage.Key, StringComparison.Ordinal)))
				{
					// If the layout has been previously rendered as part of this view, we're potentially in a layout
					// rendering cycle.
					throw new InvalidOperationException($"Layout {layoutPage.Key} has circular reference");
				}

				// Notify the previous page that any writes that are performed on it are part of sections being written
				// in the layout.
				previousPage.IsLayoutBeingRendered = true;
				layoutPage.PreviousSectionWriters = previousPage.SectionWriters;
				layoutPage.BodyContent = bodyWriter.Buffer;
				bodyWriter = await RenderPageAsync(layoutPage, context, invokeViewStarts: false).ConfigureAwait(false);

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
						await bodyWriter.Buffer.WriteToAsync(writer, _htmlEncoder).ConfigureAwait(false);
					}
				}
				else
				{
					// This means we're writing to another buffer. Use MoveTo to combine them.
					bodyWriter.Buffer.MoveTo(viewBufferTextWriter.Buffer);
				}
			}
		}

		private void ExecutePageCallbacks(ITemplatePage page, IList<Action<ITemplatePage>> callbacks)
		{
			if (callbacks?.Count > 0)
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
	}
}
