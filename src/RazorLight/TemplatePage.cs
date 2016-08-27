using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using RazorLight.Internal;
using RazorLight.Text;
using RazorLight.Templating;

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
		private AttributeInfo _attributeInfo;

		protected TemplatePage()
		{
			SectionWriters = new Dictionary<string, RenderAsyncDelegate>(StringComparer.OrdinalIgnoreCase);
		}

		public PageContext PageContext { get; set; }

		public IPageLookup PageLookup { get; set; }

		public HtmlEncoder HtmlEncoder { get; set; } = HtmlEncoder.Default;

		public virtual void SetModel(object data)
		{
		}

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

		/// <summary>
		/// Returns the specified string as a raw string. This will ensure it is not encoded.
		/// </summary>
		/// <param name="rawString">The raw string to write.</param>
		/// <returns>An instance of <see cref="IRawString"/>.</returns>
		public IRawString Raw(string rawString)
		{
			return new RawString(rawString);
		}

		/// <summary>
		/// Includes the template with the specified key
		/// </summary>
		/// <param name="key">Key used to resolve a template</param>
		/// <param name="model">Template model</param>
		public async Task IncludeAsync(string key, object model = null)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (this.PageLookup == null)
			{
				throw new RazorLightException("Can't locate a page as PageLookup is not set");
			}

			PageLookupResult pageResult = PageLookup.GetPage(key);
			if (pageResult.Success)
			{
				TemplatePage page = pageResult.ViewEntry.PageFactory();
				page.PageContext = new PageContext(this.PageContext.ViewBag) { Writer = this.PageContext.Writer };

				if (model != null)
				{
					var modelTypeInfo = new ModelTypeInfo(model.GetType());
					page.PageContext.ModelTypeInfo = modelTypeInfo;

					object pageModel = modelTypeInfo.CreateTemplateModel(model);
					page.SetModel(pageModel);
				}

				await page.ExecuteAsync();
			}
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

			var rawContent = value as IRawString;
			if (rawContent != null)
			{
				var nullEncoder = NullHtmlEncoder.Default;
				WriteTo(writer, nullEncoder, rawContent.ToEncodedString());

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

		public virtual void BeginWriteAttribute(
			string name,
			string prefix,
			int prefixOffset,
			string suffix,
			int suffixOffset,
			int attributeValuesCount)
		{
			BeginWriteAttributeTo(Output, name, prefix, prefixOffset, suffix, suffixOffset, attributeValuesCount);
		}

		public virtual void BeginWriteAttributeTo(
			TextWriter writer,
			string name,
			string prefix,
			int prefixOffset,
			string suffix,
			int suffixOffset,
			int attributeValuesCount)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			if (prefix == null)
			{
				throw new ArgumentNullException(nameof(prefix));
			}

			if (suffix == null)
			{
				throw new ArgumentNullException(nameof(suffix));
			}

			_attributeInfo = new AttributeInfo(name, prefix, prefixOffset, suffix, suffixOffset, attributeValuesCount);

			// Single valued attributes might be omitted in entirety if it the attribute value strictly evaluates to
			// null  or false. Consequently defer the prefix generation until we encounter the attribute value.
			if (attributeValuesCount != 1)
			{
				WritePositionTaggedLiteral(writer, prefix, prefixOffset);
			}
		}

		private void WritePositionTaggedLiteral(TextWriter writer, string value, int position)
		{
			//BeginContext(position, value.Length, isLiteral: true);
			WriteLiteralTo(writer, value);
			//EndContext();
		}

		public void WriteAttributeValue(
			string prefix,
			int prefixOffset,
			object value,
			int valueOffset,
			int valueLength,
			bool isLiteral)
		{
			WriteAttributeValueTo(Output, prefix, prefixOffset, value, valueOffset, valueLength, isLiteral);
		}

		public void WriteAttributeValueTo(
			TextWriter writer,
			string prefix,
			int prefixOffset,
			object value,
			int valueOffset,
			int valueLength,
			bool isLiteral)
		{
			if (_attributeInfo.AttributeValuesCount == 1)
			{
				if (IsBoolFalseOrNullValue(prefix, value))
				{
					// Value is either null or the bool 'false' with no prefix; don't render the attribute.
					_attributeInfo.Suppressed = true;
					return;
				}

				// We are not omitting the attribute. Write the prefix.
				WritePositionTaggedLiteral(writer, _attributeInfo.Prefix, _attributeInfo.PrefixOffset);

				if (IsBoolTrueWithEmptyPrefixValue(prefix, value))
				{
					// The value is just the bool 'true', write the attribute name instead of the string 'True'.
					value = _attributeInfo.Name;
				}
			}

			// This block handles two cases.
			// 1. Single value with prefix.
			// 2. Multiple values with or without prefix.
			if (value != null)
			{
				if (!string.IsNullOrEmpty(prefix))
				{
					WritePositionTaggedLiteral(writer, prefix, prefixOffset);
				}

				//BeginContext(valueOffset, valueLength, isLiteral);

				WriteUnprefixedAttributeValueTo(writer, value, isLiteral);

				//EndContext();
			}
		}

		private void WriteUnprefixedAttributeValueTo(TextWriter writer, object value, bool isLiteral)
		{
			var stringValue = value as string;

			// The extra branching here is to ensure that we call the Write*To(string) overload where possible.
			if (isLiteral && stringValue != null)
			{
				WriteLiteralTo(writer, stringValue);
			}
			else if (isLiteral)
			{
				WriteLiteralTo(writer, value);
			}
			else if (stringValue != null)
			{
				WriteTo(writer, stringValue);
			}
			else
			{
				WriteTo(writer, value);
			}
		}

		public virtual void EndWriteAttribute()
		{
			EndWriteAttributeTo(Output);
		}

		public virtual void EndWriteAttributeTo(TextWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			if (!_attributeInfo.Suppressed)
			{
				WritePositionTaggedLiteral(writer, _attributeInfo.Suffix, _attributeInfo.SuffixOffset);
			}
		}

		private struct AttributeInfo
		{
			public AttributeInfo(
				string name,
				string prefix,
				int prefixOffset,
				string suffix,
				int suffixOffset,
				int attributeValuesCount)
			{
				Name = name;
				Prefix = prefix;
				PrefixOffset = prefixOffset;
				Suffix = suffix;
				SuffixOffset = suffixOffset;
				AttributeValuesCount = attributeValuesCount;

				Suppressed = false;
			}

			public int AttributeValuesCount { get; }

			public string Name { get; }

			public string Prefix { get; }

			public int PrefixOffset { get; }

			public string Suffix { get; }

			public int SuffixOffset { get; }

			public bool Suppressed { get; set; }
		}

		#endregion

		#region "Helpers"

		private bool IsBoolTrueWithEmptyPrefixValue(string prefix, object value)
		{
			// If the value is just the bool 'true', use the attribute name as the value.
			return string.IsNullOrEmpty(prefix) &&
				(value is bool && (bool)value);
		}

		private bool IsBoolFalseOrNullValue(string prefix, object value)
		{
			return string.IsNullOrEmpty(prefix) &&
				(value == null ||
				(value is bool && !(bool)value));
		}

		#endregion

		#region "Sections"

		private void EnsureMethodCanBeInvoked(string methodName)
		{
			if (PreviousSectionWriters == null)
			{
				string message = $"{methodName} invocation is invalid";
				throw new InvalidOperationException(message);
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
					throw new InvalidOperationException($"The following sections have been defined but have not been rendered :'{ sectionNames }");
				}
			}
			else if (BodyContent != null && !_renderedBody && !_ignoreBody)
			{
				// There are no sections defined, but RenderBody was NOT called.
				// If a body was defined and the body not ignored, then RenderBody should have been called.
				var message = "Render body has not been called. To ignore call IgnoreBody().";
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
				throw new InvalidOperationException($"Section '{name}' is already defined");
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
				var message = $"Section '{sectionName}' is already rendered";
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
				throw new InvalidOperationException($"Layout page cannot find section '{sectionName}' in the content page");
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
				throw new InvalidOperationException($"Layout page cannot find section '{sectionName}' in the content page");
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
				throw new InvalidOperationException("Layout page cannot be rendered after 'FlushAsync' has been invoked.");
			}

			await Output.FlushAsync();
			return HtmlString.Empty;
		}


		#endregion
	}
}
