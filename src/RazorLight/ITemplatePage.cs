using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using RazorLight.Internal;
using RazorLight.Templating;
using RazorLight.Text;

namespace RazorLight
{
	public interface ITemplatePage {
		PageContext PageContext { get; set; }
		IPageLookup PageLookup { get; set; }
		HtmlEncoder HtmlEncoder { get; set; }
		TextWriter Output { get; }
		string Path { get; set; }
		string Layout { get; set; }
		bool IsLayoutBeingRendered { get; set; }
		IHtmlContent BodyContent { get; set; }
		Dictionary<string, RenderAsyncDelegate> SectionWriters { get; set; }
		IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }
		/// <summary>
		/// Gets the dynamic view data dictionary.
		/// </summary>
		dynamic ViewBag { get; }
		void SetModel(object data);
		Task ExecuteAsync();

		/// <summary>
		/// In a Razor layout page, ignores rendering the portion of a content page that is not within a named section.
		/// </summary>
		void IgnoreBody();

		/// <summary>
		/// Returns the specified string as a raw string. This will ensure it is not encoded.
		/// </summary>
		/// <param name="rawString">The raw string to write.</param>
		/// <returns>An instance of <see cref="IRawString"/>.</returns>
		IRawString Raw(string rawString);

		/// <summary>
		/// Includes the template with the specified key
		/// </summary>
		/// <param name="key">Key used to resolve a template</param>
		/// <param name="model">Template model</param>
		Task IncludeAsync(string key, object model = null);

		void Write(object value);
		void WriteTo(TextWriter writer, object value);
		void WriteTo(TextWriter writer, string value);
		void WriteLiteral(object value);
		void WriteLiteralTo(TextWriter writer, object value);
		void WriteLiteralTo(TextWriter writer, string value);

		void BeginWriteAttribute(
			string name,
			string prefix,
			int prefixOffset,
			string suffix,
			int suffixOffset,
			int attributeValuesCount);

		void BeginWriteAttributeTo(
			TextWriter writer,
			string name,
			string prefix,
			int prefixOffset,
			string suffix,
			int suffixOffset,
			int attributeValuesCount);

		void WriteAttributeValue(
			string prefix,
			int prefixOffset,
			object value,
			int valueOffset,
			int valueLength,
			bool isLiteral);

		void WriteAttributeValueTo(
			TextWriter writer,
			string prefix,
			int prefixOffset,
			object value,
			int valueOffset,
			int valueLength,
			bool isLiteral);

		void EndWriteAttribute();
		void EndWriteAttributeTo(TextWriter writer);
		void EnsureRenderedBodyOrSections();

		/// <summary>
		/// Creates a named content section in the page that can be invoked in a Layout page using
		/// <see cref="TemplatePage.RenderSection(string)"/> or <see cref="TemplatePage.RenderSectionAsync(string,bool)"/>.
		/// </summary>
		/// <param name="name">The name of the section to create.</param>
		/// <param name="section">The <see cref="RenderAsyncDelegate"/> to execute when rendering the section.</param>
		void DefineSection(string name, RenderAsyncDelegate section);

		/// <summary>
		/// Returns a value that indicates whether the specified section is defined in the content page.
		/// </summary>
		/// <param name="name">The section name to search for.</param>
		/// <returns><c>true</c> if the specified section is defined in the content page; otherwise, <c>false</c>.</returns>
		bool IsSectionDefined(string name);

		/// <summary>
		/// In layout pages, renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the section to render.</param>
		/// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="TemplatePage.Write"/> call to
		/// succeed.</returns>
		/// <remarks>The method writes to the <see cref="TemplatePage.Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		HtmlString RenderSection(string name);

		/// <summary>
		/// In layout pages, renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The section to render.</param>
		/// <param name="required">Indicates if this section must be rendered.</param>
		/// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="TemplatePage.Write"/> call to
		/// succeed.</returns>
		/// <remarks>The method writes to the <see cref="TemplatePage.Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		HtmlString RenderSection(string name, bool required);

		/// <summary>
		/// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The section to render.</param>
		/// <returns>A <see cref="Task{HtmlString}"/> that on completion returns <see cref="HtmlString.Empty"/> that
		/// allows the <see cref="TemplatePage.Write"/> call to succeed.</returns>
		/// <remarks>The method writes to the <see cref="TemplatePage.Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		Task<HtmlString> RenderSectionAsync(string name);

		/// <summary>
		/// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The section to render.</param>
		/// <param name="required">Indicates the <paramref name="name"/> section must be registered
		/// (using <c>@section</c>) in the page.</param>
		/// <returns>A <see cref="Task{HtmlString}"/> that on completion returns <see cref="HtmlString.Empty"/> that
		/// allows the <see cref="TemplatePage.Write"/> call to succeed.</returns>
		/// <remarks>The method writes to the <see cref="TemplatePage.Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		/// <exception cref="InvalidOperationException">if <paramref name="required"/> is <c>true</c> and the section
		/// was not registered using the <c>@section</c> in the Razor page.</exception>
		Task<HtmlString> RenderSectionAsync(string name, bool required);

		/// <summary>
		/// In layout pages, ignores rendering the content of the section named <paramref name="sectionName"/>.
		/// </summary>
		/// <param name="sectionName">The section to ignore.</param>
		void IgnoreSection(string sectionName);

		/// <summary>
		/// Invokes <see cref="TextWriter.FlushAsync"/> on <see cref="TemplatePage.Output"/> and <see cref="M:Stream.FlushAsync"/>
		/// on the response stream, writing out any buffered content to the <see cref="HttpResponse.Body"/>.
		/// </summary>
		/// <returns>A <see cref="Task{HtmlString}"/> that represents the asynchronous flush operation and on
		/// completion returns <see cref="HtmlString.Empty"/>.</returns>
		/// <remarks>The value returned is a token value that allows FlushAsync to work directly in an HTML
		/// section. However the value does not represent the rendered content.
		/// This method also writes out headers, so any modifications to headers must be done before
		/// <see cref="TemplatePage.FlushAsync"/> is called. For example, call <see cref="SetAntiforgeryCookieAndHeader"/> to send
		/// antiforgery cookie token and X-Frame-Options header to client before this method flushes headers out.
		/// </remarks>
		Task<HtmlString> FlushAsync();
	}
}