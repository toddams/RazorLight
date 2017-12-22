using Microsoft.AspNetCore.Html;
using RazorLight.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorLight
{
	public interface ITemplatePage
	{
		void SetModel(object model);

		/// <summary>
		/// Gets or sets the view context of the rendering template.
		/// </summary>
		PageContext PageContext { get; set; }

		/// <summary>
		/// Gets or sets the body content.
		/// </summary>
		IHtmlContent BodyContent { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether encoding is disabled for the entire template
		/// </summary>
		bool DisableEncoding { get; set; }

		/// <summary>
		/// Gets or sets the unique key of the current template
		/// </summary>
		string Key { get; set; }

		/// <summary>
		/// Gets or sets a flag that determines if the layout of this page is being rendered.
		/// </summary>
		/// <remarks>
		/// Sections defined in a page are deferred and executed as part of the layout page.
		/// When this flag is set, all write operations performed by the page are part of a
		/// section being rendered.
		/// </remarks>
		bool IsLayoutBeingRendered { get; set; }

		/// <summary>
		/// Gets or sets the key of a layout page.
		/// </summary>
		string Layout { get; set; }

		/// <summary>
		/// Gets or sets the sections that can be rendered by this page.
		/// </summary>
		IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

		/// <summary>
		/// Gets the sections that are defined by this page.
		/// </summary>
		IDictionary<string, RenderAsyncDelegate> SectionWriters { get; }

		/// <summary>
		/// Renders the page and writes the output to the <see cref="IPageContext.Writer"/> />.
		/// </summary>
		/// <returns>A task representing the result of executing the page.</returns>
		Task ExecuteAsync();

		Func<string, object, Task> IncludeFunc { get; set; }

		void EnsureRenderedBodyOrSections();
	}
}
