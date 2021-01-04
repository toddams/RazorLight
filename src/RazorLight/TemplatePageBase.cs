using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using System.IO;
using System.ComponentModel;
using System.Text.Encodings.Web;
using System.Diagnostics;
using RazorLight.Internal;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using RazorLight.TagHelpers;
using System.Buffers;
using RazorLight.Internal.Buffering;
using RazorLight.Text;

namespace RazorLight
{
	public abstract class TemplatePageBase : ITemplatePage
	{
		private readonly Stack<TextWriter> _textWriterStack = new Stack<TextWriter>();
		private StringWriter _valueBuffer;
		private ITagHelperFactory _tagHelperFactory;
		private IViewBufferScope _bufferScope;
		private TextWriter _pageWriter;
		private AttributeInfo _attributeInfo;
		private TagHelperAttributeInfo _tagHelperAttributeInfo;
		//private IUrlHelper _urlHelper;

		public abstract void SetModel(object model);

		/// <inheritdoc />
		public virtual PageContext PageContext { get; set; }

		/// <inheritdoc />
		public IHtmlContent BodyContent { get; set; }

		/// <inheritdoc />
		public bool IsLayoutBeingRendered { get; set; }

		/// <inheritdoc />
		public string Layout { get; set; }

		public virtual dynamic ViewBag
		{
			get
			{
				if (PageContext == null)
				{
					throw new InvalidOperationException();
				}

				return PageContext.ViewBag;
			}
		}

		public Func<string, object, Task> IncludeFunc { get; set; }

		private Stack<TagHelperScopeInfo> TagHelperScopes { get; } = new Stack<TagHelperScopeInfo>();

		/// <inheritdoc />
		public IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

		/// <inheritdoc />
		public IDictionary<string, RenderAsyncDelegate> SectionWriters { get; } =
			new Dictionary<string, RenderAsyncDelegate>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets the <see cref="System.Text.Encodings.Web.HtmlEncoder"/> to use when this template />
		/// handles non-<see cref="IHtmlContent"/> C# expressions.
		/// </summary>
		public HtmlEncoder HtmlEncoder { get; set; } = HtmlEncoder.Default;

		/// <inheritdoc />
		public string Key { get; set; }

		public bool DisableEncoding { get; set; } = false;

		/// <summary>
		/// Gets the <see cref="TextWriter"/> that the template is writing output to.
		/// </summary>
		public virtual TextWriter Output
		{
			get
			{
				if (PageContext == null)
				{
					throw new InvalidOperationException();
				}

				return PageContext.Writer;
			}
		}

		//TODO: pass factory somewhere (was taken from Services)
		private ITagHelperFactory TagHelperFactory
		{
			get
			{
				if (_tagHelperFactory == null)
				{
					//var services = ViewContext.HttpContext.RequestServices;
					//_tagHelperFactory = services.GetRequiredService<ITagHelperFactory>();
					_tagHelperFactory = new DefaultTagHelperFactory(new DefaultTagHelperActivator(new TypeActivatorCache())); //TODO: replace cache with cached instance
				}

				return _tagHelperFactory;
			}
		}

		private IViewBufferScope BufferScope
		{
			get
			{
				if (_bufferScope == null)
				{
					//TODO: replace with services maybe
					//var services = ViewContext.HttpContext.RequestServices;
					//_bufferScope = services.GetRequiredService<IViewBufferScope>();
					_bufferScope = new MemoryPoolViewBufferScope(ArrayPool<ViewBufferValue>.Shared, ArrayPool<char>.Shared);
				}

				return _bufferScope;
			}
		}

		/// <inheritdoc />
		public abstract Task ExecuteAsync();

		/// <summary>
		/// Invokes <see cref="TextWriter.FlushAsync"/> on <see cref="Output"/> and <see cref="m:Stream.FlushAsync"/>
		/// on the response stream, writing out any buffered content to the <see cref="Microsoft.AspNetCore.Http.HttpResponse.Body"/>.
		/// </summary>
		/// <returns>A <see cref="Task{HtmlString}"/> that represents the asynchronous flush operation and on
		/// completion returns an empty <see cref="IHtmlContent"/>.</returns>
		/// <remarks>The value returned is a token value that allows FlushAsync to work directly in an HTML
		/// section. However the value does not represent the rendered content.
		/// This method also writes out headers, so any modifications to headers must be done before
		/// <see cref="FlushAsync"/> is called. For example, call <see cref="M:Microsoft.AspNetCore.Mvc.Razor.RazorPageBase.SetAntiforgeryCookieAndHeader"/> to send
		/// antiforgery cookie token and X-Frame-Options header to client before this method flushes headers out.
		/// </remarks>
		public virtual async Task<HtmlString> FlushAsync()
		{
			// If there are active scopes, then we should throw. Cannot flush content that has the potential to change.
			if (TagHelperScopes.Count > 0)
			{
				throw new InvalidOperationException();
			}

			// Calls to Flush are allowed if the page does not specify a Layout or if it is executing a section in the
			// Layout.
			if (!IsLayoutBeingRendered && !string.IsNullOrEmpty(Layout))
			{
				throw new InvalidOperationException();
			}

			await Output.FlushAsync();
			return HtmlString.Empty;
		}

		public abstract void BeginContext(int position, int length, bool isLiteral);

		public abstract void EndContext();

		public abstract void EnsureRenderedBodyOrSections();

		/// <summary>
		/// Returns the specified string as a raw string. This will ensure it is not encoded.
		/// </summary>
		/// <param name="rawString">The raw string to write.</param>
		/// <returns>An instance of <see cref="IRawString"/>.</returns>
		public IRawString Raw(string rawString)
		{
			return new RawString(rawString);
		}

		public static IHtmlContent HelperFunction(Func<object, IHtmlContent> body)
		{
			return body(null);
		}

		#region Tag helpers

		/// <summary>
		/// Creates and activates a <see cref="ITagHelper"/>.
		/// </summary>
		/// <typeparam name="TTagHelper">A <see cref="ITagHelper"/> type.</typeparam>
		/// <returns>The activated <see cref="ITagHelper"/>.</returns>
		/// <remarks>
		/// <typeparamref name="TTagHelper"/> must have a parameterless constructor.
		/// </remarks>
		public TTagHelper CreateTagHelper<TTagHelper>() where TTagHelper : ITagHelper
		{
			return TagHelperFactory.CreateTagHelper<TTagHelper>(PageContext);
		}

		/// <summary>
		/// Starts a new writing scope and optionally overrides <see cref="HtmlEncoder"/> within that scope.
		/// </summary>
		/// <param name="encoder">
		/// The <see cref="System.Text.Encodings.Web.HtmlEncoder"/> to use when this <see cref="TemplatePageBase"/> handles
		/// non-<see cref="IHtmlContent"/> C# expressions. If <c>null</c>, does not change <see cref="HtmlEncoder"/>.
		/// </param>
		/// <remarks>
		/// All writes to the <see cref="Output"/> or <see cref="M:PageContext.Writer"/> after calling this method will
		/// be buffered until <see cref="EndTagHelperWritingScope"/> is called.
		/// </remarks>
		public void StartTagHelperWritingScope(HtmlEncoder encoder)
		{
			var buffer = new ViewBuffer(BufferScope, Key, ViewBuffer.TagHelperPageSize);
			TagHelperScopes.Push(new TagHelperScopeInfo(buffer, HtmlEncoder, PageContext.Writer));

			// If passed an HtmlEncoder, override the property.
			if (encoder != null)
			{
				HtmlEncoder = encoder;
			}

			// We need to replace the ViewContext's Writer to ensure that all content (including content written
			// from HTML helpers) is redirected.
			PageContext.Writer = new ViewBufferTextWriter(buffer, PageContext.Writer.Encoding);
		}

		/// <summary>
		/// Ends the current writing scope that was started by calling <see cref="StartTagHelperWritingScope"/>.
		/// </summary>
		/// <returns>The buffered <see cref="TagHelperContent"/>.</returns>
		public TagHelperContent EndTagHelperWritingScope()
		{
			if (TagHelperScopes.Count == 0)
			{
				throw new InvalidOperationException("There is no active scope to write");
			}

			var scopeInfo = TagHelperScopes.Pop();

			// Get the content written during the current scope.
			var tagHelperContent = new DefaultTagHelperContent();
			_ = tagHelperContent.AppendHtml(scopeInfo.Buffer);

			// Restore previous scope.
			HtmlEncoder = scopeInfo.HtmlEncoder;
			PageContext.Writer = scopeInfo.Writer;

			return tagHelperContent;
		}

		/// <summary>
		/// Starts a new scope for writing <see cref="ITagHelper"/> attribute values.
		/// </summary>
		/// <remarks>
		/// All writes to the <see cref="Output"/> or <see cref="M:PageContext.Writer"/> after calling this method will
		/// be buffered until <see cref="EndWriteTagHelperAttribute"/> is called.
		/// The content will be buffered using a shared <see cref="StringWriter"/> within this <see cref="TemplatePageBase"/>
		/// Nesting of <see cref="BeginWriteTagHelperAttribute"/> and <see cref="EndWriteTagHelperAttribute"/> method calls
		/// is not supported.
		/// </remarks>
		public void BeginWriteTagHelperAttribute()
		{
			if (_pageWriter != null)
			{
				throw new InvalidOperationException("Nesting of attribute writing scope is not supported");
			}

			_pageWriter = PageContext.Writer;

			if (_valueBuffer == null)
			{
				_valueBuffer = new StringWriter();
			}

			// We need to replace the ViewContext's Writer to ensure that all content (including content written
			// from HTML helpers) is redirected.
			PageContext.Writer = _valueBuffer;

		}

		/// <summary>
		/// Ends the current writing scope that was started by calling <see cref="BeginWriteTagHelperAttribute"/>.
		/// </summary>
		/// <returns>The content buffered by the shared <see cref="StringWriter"/> of this <see cref="TemplatePage"/>.</returns>
		/// <remarks>
		/// This method assumes that there will be no nesting of <see cref="BeginWriteTagHelperAttribute"/>
		/// and <see cref="EndWriteTagHelperAttribute"/> method calls.
		/// </remarks>
		public string EndWriteTagHelperAttribute()
		{
			if (_pageWriter == null)
			{
				throw new InvalidOperationException("There is no active writing scope to end");
			}

			var content = _valueBuffer.ToString();
			_valueBuffer.GetStringBuilder().Clear();

			// Restore previous writer.
			PageContext.Writer = _pageWriter;
			_pageWriter = null;

			return content;
		}

		#endregion

		/*
        public virtual string Href(string contentPath)
        {
            if (contentPath == null)
            {
                throw new ArgumentNullException(nameof(contentPath));
            }

            if (_urlHelper == null)
            {
                var services = ViewContext?.HttpContext.RequestServices;
                var factory = services.GetRequiredService<IUrlHelperFactory>();
                _urlHelper = factory.GetUrlHelper(PageContext);
            }

            return _urlHelper.Content(contentPath);
        }*/

		/// <summary>
		/// Creates a named content section in the page that can be invoked in a Layout page using
		/// <c>RenderSection</c> or <c>RenderSectionAsync</c>
		/// </summary>
		/// <param name="name">The name of the section to create.</param>
		/// <param name="section">The delegate to execute when rendering the section.</param>
		/// <remarks>This is a temporary placeholder method to support ASP.NET Core 2.0.0 editor code generation.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected void DefineSection(string name, Func<object, Task> section)
			=> DefineSection(name, () => section(null /* writer */));

		/// <summary>
		/// Creates a named content section in the page that can be invoked in a Layout page using
		/// <c>RenderSection</c> or <c>RenderSectionAsync</c>
		/// </summary>
		/// <param name="name">The name of the section to create.</param>
		/// <param name="section">The <see cref="RenderAsyncDelegate"/> to execute when rendering the section.</param>
		public virtual void DefineSection(string name, RenderAsyncDelegate section)
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
				throw new InvalidOperationException();
			}
			SectionWriters[name] = section;
		}

		#region Write section

		/// <summary>
		/// Writes the specified <paramref name="value"/> with HTML encoding to <see cref="Output"/>.
		/// </summary>
		/// <param name="value">The <see cref="object"/> to write.</param>
		public virtual void Write(object value)
		{
			if (value == null || value == HtmlString.Empty)
			{
				return;
			}

			var writer = Output;
			var encoder = HtmlEncoder;

			switch (value)
			{
				case IRawString raw:
					raw.WriteTo(writer);
					break;
				case IHtmlContent html:
					var bufferedWriter = writer as ViewBufferTextWriter;
					if (bufferedWriter == null || !bufferedWriter.IsBuffering)
					{
						html.WriteTo(writer, encoder);
					}
					else
					{
						if (value is IHtmlContentContainer htmlContentContainer)
						{
							// This is likely another ViewBuffer.
							htmlContentContainer.MoveTo(bufferedWriter.Buffer);
						}
						else
						{
							// Perf: This is the common case for IHtmlContent, ViewBufferTextWriter is inefficient
							// for writing character by character.
							_ = bufferedWriter.Buffer.AppendHtml(html);
						}
					}
					break;
				default:
					Write(value.ToString());
					break;
			}
		}

		/// <summary>
		/// Writes the specified <paramref name="value"/> with HTML encoding to <see cref="Output"/>.
		/// </summary>
		/// <param name="value">The <see cref="string"/> to write.</param>
		public virtual void Write(string value)
		{
			var writer = Output;
			var encoder = HtmlEncoder;
			if (!string.IsNullOrEmpty(value))
			{
				// Perf: Encode right away instead of writing it character-by-character.
				// character-by-character isn't efficient when using a writer backed by a ViewBuffer.
				var encoded = DisableEncoding ? value : encoder.Encode(value);
				writer.Write(encoded);
			}
		}

		/// <summary>
		/// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
		/// </summary>
		/// <param name="value">The <see cref="object"/> to write.</param>
		public virtual void WriteLiteral(object value)
		{
			if (value == null)
			{
				return;
			}

			WriteLiteral(value.ToString());
		}

		/// <summary>
		/// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
		/// </summary>
		/// <param name="value">The <see cref="string"/> to write.</param>
		public virtual void WriteLiteral(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				Output.Write(value);
			}
		}

		// Internal for unit testing.
		protected internal virtual void PushWriter(TextWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			_textWriterStack.Push(PageContext.Writer);
			PageContext.Writer = writer;
		}

		// Internal for unit testing.
		protected internal virtual TextWriter PopWriter()
		{
			PageContext.Writer = _textWriterStack.Pop();
			return PageContext.Writer;
		}

		public virtual void BeginWriteAttribute(
			string name,
			string prefix,
			int prefixOffset,
			string suffix,
			int suffixOffset,
			int attributeValuesCount)
		{
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
				WritePositionTaggedLiteral(prefix, prefixOffset);
			}
		}

		public void WriteAttributeValue(
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
				WritePositionTaggedLiteral(_attributeInfo.Prefix, _attributeInfo.PrefixOffset);

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
					WritePositionTaggedLiteral(prefix, prefixOffset);
				}

				BeginContext(valueOffset, valueLength, isLiteral);

				WriteUnprefixedAttributeValue(value, isLiteral);

				EndContext();
			}
		}

		public virtual void EndWriteAttribute()
		{
			if (!_attributeInfo.Suppressed)
			{
				WritePositionTaggedLiteral(_attributeInfo.Suffix, _attributeInfo.SuffixOffset);
			}
		}

		public void BeginAddHtmlAttributeValues(
			TagHelperExecutionContext executionContext,
			string attributeName,
			int attributeValuesCount,
			HtmlAttributeValueStyle attributeValueStyle)
		{
			_tagHelperAttributeInfo = new TagHelperAttributeInfo(
				executionContext,
				attributeName,
				attributeValuesCount,
				attributeValueStyle);
		}

		public void AddHtmlAttributeValue(
			string prefix,
			int prefixOffset,
			object value,
			int valueOffset,
			int valueLength,
			bool isLiteral)
		{
			Debug.Assert(_tagHelperAttributeInfo.ExecutionContext != null);
			if (_tagHelperAttributeInfo.AttributeValuesCount == 1)
			{
				if (IsBoolFalseOrNullValue(prefix, value))
				{
					// The first value was 'null' or 'false' indicating that we shouldn't render the attribute. The
					// attribute is treated as a TagHelper attribute so it's only available in
					// TagHelperContext.AllAttributes for TagHelper authors to see (if they want to see why the
					// attribute was removed from TagHelperOutput.Attributes).
					_tagHelperAttributeInfo.ExecutionContext.AddTagHelperAttribute(
						_tagHelperAttributeInfo.Name,
						value?.ToString() ?? string.Empty,
						_tagHelperAttributeInfo.AttributeValueStyle);
					_tagHelperAttributeInfo.Suppressed = true;
					return;
				}
				else if (IsBoolTrueWithEmptyPrefixValue(prefix, value))
				{
					_tagHelperAttributeInfo.ExecutionContext.AddHtmlAttribute(
						_tagHelperAttributeInfo.Name,
						_tagHelperAttributeInfo.Name,
						_tagHelperAttributeInfo.AttributeValueStyle);
					_tagHelperAttributeInfo.Suppressed = true;
					return;
				}
			}

			if (value != null)
			{
				// Perf: We'll use this buffer for all of the attribute values and then clear it to
				// reduce allocations.
				if (_valueBuffer == null)
				{
					_valueBuffer = new StringWriter();
				}

				PushWriter(_valueBuffer);
				if (!string.IsNullOrEmpty(prefix))
				{
					WriteLiteral(prefix);
				}

				WriteUnprefixedAttributeValue(value, isLiteral);
				PopWriter();
			}
		}

		public void EndAddHtmlAttributeValues(TagHelperExecutionContext executionContext)
		{
			if (!_tagHelperAttributeInfo.Suppressed)
			{
				// Perf: _valueBuffer might be null if nothing was written. If it is set, clear it so
				// it is reset for the next value.
				var content = _valueBuffer == null ? HtmlString.Empty : new HtmlString(_valueBuffer.ToString());
				_valueBuffer?.GetStringBuilder().Clear();

				executionContext.AddHtmlAttribute(_tagHelperAttributeInfo.Name, content, _tagHelperAttributeInfo.AttributeValueStyle);
			}
		}

		private void WriteUnprefixedAttributeValue(object value, bool isLiteral)
		{
			var stringValue = value as string;

			// The extra branching here is to ensure that we call the Write*To(string) overload where possible.
			if (isLiteral && stringValue != null)
			{
				WriteLiteral(stringValue);
			}
			else if (isLiteral)
			{
				WriteLiteral(value);
			}
			else if (stringValue != null)
			{
				Write(stringValue);
			}
			else
			{
				Write(value);
			}
		}

		private void WritePositionTaggedLiteral(string value, int position)
		{
			BeginContext(position, value.Length, isLiteral: true);
			WriteLiteral(value);
			EndContext();
		}

		#endregion

		#region Helpers

		private bool IsBoolFalseOrNullValue(string prefix, object value)
		{
			return string.IsNullOrEmpty(prefix) &&
				(value == null ||
				(value is bool && !(bool)value));
		}

		private bool IsBoolTrueWithEmptyPrefixValue(string prefix, object value)
		{
			// If the value is just the bool 'true', use the attribute name as the value.
			return string.IsNullOrEmpty(prefix) &&
				(value is bool && (bool)value);
		}

		#endregion

		#region structs

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

		private struct TagHelperAttributeInfo
		{
			public TagHelperAttributeInfo(
				TagHelperExecutionContext tagHelperExecutionContext,
				string name,
				int attributeValuesCount,
				HtmlAttributeValueStyle attributeValueStyle)
			{
				ExecutionContext = tagHelperExecutionContext;
				Name = name;
				AttributeValuesCount = attributeValuesCount;
				AttributeValueStyle = attributeValueStyle;

				Suppressed = false;
			}

			public string Name { get; }

			public TagHelperExecutionContext ExecutionContext { get; }

			public int AttributeValuesCount { get; }

			public HtmlAttributeValueStyle AttributeValueStyle { get; }

			public bool Suppressed { get; set; }
		}

		private struct TagHelperScopeInfo
		{
			public TagHelperScopeInfo(ViewBuffer buffer, HtmlEncoder encoder, TextWriter writer)
			{
				Buffer = buffer;
				HtmlEncoder = encoder;
				Writer = writer;
			}

			public ViewBuffer Buffer { get; }

			public HtmlEncoder HtmlEncoder { get; }

			public TextWriter Writer { get; }
		}

		#endregion
	}
}
