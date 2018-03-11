using System;
using System.Collections.Generic;
using System.Text;

namespace RazorLight.ViewFeatures
{
	/// <summary>
	/// Contract for contextualizing a property activated by a view with the <see cref="PageContext"/>.
	/// </summary>
	/// <remarks>This interface is used for contextualizing properties added to a Razor page using <c>@inject</c>.</remarks>
	public interface IPageContextAware
	{
		/// <summary>
		/// Contextualizes the instance with the specified <paramref name="pageContext"/>.
		/// </summary>
		/// <param name="pageContext">The <see cref="PageContext"/>.</param>
		void Contextualize(PageContext pageContext);
	}
}
