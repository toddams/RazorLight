using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorLight.TagHelpers
{
	/// <summary>
	/// Provides methods to create a tag helper.
	/// </summary>
	public interface ITagHelperActivator
	{
		/// <summary>
		/// Creates an <see cref="ITagHelper"/>.
		/// </summary>
		/// <typeparam name="TTagHelper">The <see cref="ITagHelper"/> type.</typeparam>
		/// <param name="context">The <see cref="PageContext"/> for the executing view.</param>
		/// <returns>The tag helper.</returns>
		TTagHelper Create<TTagHelper>(PageContext context) where TTagHelper : ITagHelper;
	}
}
