using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using RazorLight.Internal;

namespace RazorLight
{
	/// <summary>
	/// Lightweight Razor page with a <see cref="TextWriter"/> for the generated output
	/// </summary>
	public abstract class TemplatePage
	{
		private readonly HashSet<string> _renderedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private HashSet<string> _ignoredSections;
		private bool _renderedBody;
		private bool _ignoreBody;

		protected TemplatePage()
		{
			SectionWriters = new Dictionary<string, RenderAsyncDelegate>(StringComparer.OrdinalIgnoreCase);
		}

		public PageContext PageContext { get; set; }

		public HtmlEncoder HtmlEncoder { get; set; } = HtmlEncoder.Default;

		public virtual TextWriter Output
		{
			get
			{
				if (PageContext == null)
				{
					throw new InvalidOperationException("Page context is not defined");
				}

				return PageContext.Writer;
			}
		}

		public string Path { get; set; }

		public string Layout { get; set; }

		public bool IsLayoutBeingRendered { get; set; }

		public IHtmlContent BodyContent { get; set; }

		public Dictionary<string, RenderAsyncDelegate> SectionWriters { get; set; }

		public IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

		/// <summary>
		/// Gets the dynamic view data dictionary.
		/// </summary>
		public dynamic ViewBag => PageContext?.ViewBag;

		public abstract Task ExecuteAsync();

		/// <summary>
		/// In a Razor layout page, renders the portion of a content page that is not within a named section.
		/// </summary>
		/// <returns>The HTML content to render.</returns>
		protected virtual IHtmlContent RenderBody()
		{
			if (BodyContent == null)
			{
				throw new InvalidOperationException($"Method {nameof(RenderBody)} cannot be called");
			}

			_renderedBody = true;
			return BodyContent;
		}

		/// <summary>
		/// In a Razor layout page, ignores rendering the portion of a content page that is not within a named section.
		/// </summary>
		public void IgnoreBody()
		{
			_ignoreBody = true;
		}

		#region "Razor writers"

		public virtual void Write(object value)
		{
			WriteTo(Output, value);
		}

		public virtual void WriteTo(TextWriter writer, object value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			WriteTo(writer, HtmlEncoder, value);
		}

		public static void WriteTo(TextWriter writer, HtmlEncoder encoder, object value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			if (encoder == null)
			{
				throw new ArgumentNullException(nameof(encoder));
			}

			if (value == null || value == HtmlString.Empty)
			{
				return;
			}

			var htmlContent = value as IHtmlContent;
			if (htmlContent != null)
			{
				htmlContent.WriteTo(writer, encoder);

				return;
			}

			WriteTo(writer, encoder, value.ToString());
		}

		public virtual void WriteTo(TextWriter writer, string value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			WriteTo(writer, HtmlEncoder, value);
		}

		private static void WriteTo(TextWriter writer, HtmlEncoder encoder, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				// Perf: Encode right away instead of writing it character-by-character.
				// character-by-character isn't efficient when using a writer backed by a ViewBuffer.
				var encoded = encoder.Encode(value);
				writer.Write(encoded);
			}
		}

		public virtual void WriteLiteral(object value)
		{
			WriteLiteralTo(Output, value);
		}

		public virtual void WriteLiteralTo(TextWriter writer, object value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			if (value != null)
			{
				WriteLiteralTo(writer, value.ToString());
			}
		}

		public virtual void WriteLiteralTo(TextWriter writer, string value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			if (!string.IsNullOrEmpty(value))
			{
				writer.Write(value);
			}
		}

		#endregion

		#region "Sections"

		private void EnsureMethodCanBeInvoked(string methodName)
		{
			if (PreviousSectionWriters == null)
			{
				throw new InvalidOperationException($"Method \"{methodName}\" cannot be called");
			}
		}

		public void EnsureRenderedBodyOrSections()
		{
			// a) all sections defined for this page are rendered.
			// b) if no sections are defined, then the body is rendered if it's available.
			if (PreviousSectionWriters != null && PreviousSectionWriters.Count > 0)
			{
				var sectionsNotRendered = PreviousSectionWriters.Keys.Except(
					_renderedSections,
					StringComparer.OrdinalIgnoreCase);

				string[] sectionsNotIgnored;
				if (_ignoredSections != null)
				{
					sectionsNotIgnored = sectionsNotRendered.Except(_ignoredSections, StringComparer.OrdinalIgnoreCase).ToArray();
				}
				else
				{
					sectionsNotIgnored = sectionsNotRendered.ToArray();
				}

				if (sectionsNotIgnored.Length > 0)
				{
					var sectionNames = string.Join(", ", sectionsNotIgnored);
					throw new InvalidOperationException($"Sections not rendered: { sectionNames }");
				}
			}
			else if (BodyContent != null && !_renderedBody && !_ignoreBody)
			{
				// There are no sections defined, but RenderBody was NOT called.
				// If a body was defined and the body not ignored, then RenderBody should have been called.
				var message = "Render body is not called";
				throw new InvalidOperationException(message);
			}
		}

		/// <summary>
		/// Creates a named content section in the page that can be invoked in a Layout page using
		/// <see cref="RenderSection(string)"/> or <see cref="RenderSectionAsync(string, bool)"/>.
		/// </summary>
		/// <param name="name">The name of the section to create.</param>
		/// <param name="section">The <see cref="RenderAsyncDelegate"/> to execute when rendering the section.</param>
		public void DefineSection(string name, RenderAsyncDelegate section)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (section == null)
			{
				throw new ArgumentNullException(nameof(section));
			}

			if (SectionWriters.ContainsKey(name))
			{
				throw new InvalidOperationException($"Section \"{name}\" is already defined");
			}

			SectionWriters[name] = section;
		}

		/// <summary>
		/// Returns a value that indicates whether the specified section is defined in the content page.
		/// </summary>
		/// <param name="name">The section name to search for.</param>
		/// <returns><c>true</c> if the specified section is defined in the content page; otherwise, <c>false</c>.</returns>
		public bool IsSectionDefined(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			EnsureMethodCanBeInvoked(nameof(IsSectionDefined));
			return PreviousSectionWriters.ContainsKey(name);
		}

		/// <summary>
		/// In layout pages, renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the section to render.</param>
		/// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="Write(object)"/> call to
		/// succeed.</returns>
		/// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		public HtmlString RenderSection(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			return RenderSection(name, required: true);
		}

		/// <summary>
		/// In layout pages, renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The section to render.</param>
		/// <param name="required">Indicates if this section must be rendered.</param>
		/// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="Write(object)"/> call to
		/// succeed.</returns>
		/// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		public HtmlString RenderSection(string name, bool required)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			EnsureMethodCanBeInvoked(nameof(RenderSection));

			Task<HtmlString> task = RenderSectionAsyncCore(name, required);
			return task.GetAwaiter().GetResult();
		}

		/// <summary>
		/// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The section to render.</param>
		/// <returns>A <see cref="Task{HtmlString}"/> that on completion returns <see cref="HtmlString.Empty"/> that
		/// allows the <see cref="Write(object)"/> call to succeed.</returns>
		/// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		public Task<HtmlString> RenderSectionAsync(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			return RenderSectionAsync(name, required: true);
		}

		/// <summary>
		/// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The section to render.</param>
		/// <param name="required">Indicates the <paramref name="name"/> section must be registered
		/// (using <c>@section</c>) in the page.</param>
		/// <returns>A <see cref="Task{HtmlString}"/> that on completion returns <see cref="HtmlString.Empty"/> that
		/// allows the <see cref="Write(object)"/> call to succeed.</returns>
		/// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		/// <exception cref="InvalidOperationException">if <paramref name="required"/> is <c>true</c> and the section
		/// was not registered using the <c>@section</c> in the Razor page.</exception>
		public Task<HtmlString> RenderSectionAsync(string name, bool required)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			EnsureMethodCanBeInvoked(nameof(RenderSectionAsync));
			return RenderSectionAsyncCore(name, required);
		}

		private async Task<HtmlString> RenderSectionAsyncCore(string sectionName, bool required)
		{
			if (_renderedSections.Contains(sectionName))
			{
				var message = $"Section {sectionName} is already rendered";
				throw new InvalidOperationException(message);
			}

			RenderAsyncDelegate renderDelegate;
			if (PreviousSectionWriters.TryGetValue(sectionName, out renderDelegate))
			{
				_renderedSections.Add(sectionName);
				await renderDelegate(Output);

				// Return a token value that allows the Write call that wraps the RenderSection \ RenderSectionAsync
				// to succeed.
				return HtmlString.Empty;
			}
			else if (required)
			{
				// If the section is not found, and it is not optional, throw an error.
				throw new InvalidOperationException($"Section {sectionName} is not defined");
			}
			else
			{
				// If the section is optional and not found, then don't do anything.
				return null;
			}
		}

		/// <summary>
		/// In layout pages, ignores rendering the content of the section named <paramref name="sectionName"/>.
		/// </summary>
		/// <param name="sectionName">The section to ignore.</param>
		public void IgnoreSection(string sectionName)
		{
			if (sectionName == null)
			{
				throw new ArgumentNullException(nameof(sectionName));
			}

			if (!PreviousSectionWriters.ContainsKey(sectionName))
			{
				// If the section is not defined, throw an error.
				throw new InvalidOperationException($"Section {sectionName} is not defined");
			}

			if (_ignoredSections == null)
			{
				_ignoredSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}

			_ignoredSections.Add(sectionName);
		}

		/// <summary>
		/// Invokes <see cref="TextWriter.FlushAsync"/> on <see cref="Output"/> and <see cref="M:Stream.FlushAsync"/>
		/// on the response stream, writing out any buffered content to the <see cref="HttpResponse.Body"/>.
		/// </summary>
		/// <returns>A <see cref="Task{HtmlString}"/> that represents the asynchronous flush operation and on
		/// completion returns <see cref="HtmlString.Empty"/>.</returns>
		/// <remarks>The value returned is a token value that allows FlushAsync to work directly in an HTML
		/// section. However the value does not represent the rendered content.
		/// This method also writes out headers, so any modifications to headers must be done before
		/// <see cref="FlushAsync"/> is called. For example, call <see cref="SetAntiforgeryCookieAndHeader"/> to send
		/// antiforgery cookie token and X-Frame-Options header to client before this method flushes headers out.
		/// </remarks>
		public async Task<HtmlString> FlushAsync()
		{
			// Calls to Flush are allowed if the page does not specify a Layout or if it is executing a section in the
			// Layout.
			if (!IsLayoutBeingRendered && !string.IsNullOrEmpty(Layout))
			{
				throw new InvalidOperationException("Layout cannot be rendered");
			}

			await Output.FlushAsync();
			return HtmlString.Empty;
		}
		
		
		#endregion
	}
}
