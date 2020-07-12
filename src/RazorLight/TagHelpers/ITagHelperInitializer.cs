using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorLight.TagHelpers
{
	/// <summary>
	/// Initializes an <see cref="ITagHelper"/> before it's executed.
	/// </summary>
	/// <typeparam name="TTagHelper">The <see cref="ITagHelper"/> type.</typeparam>
	public interface ITagHelperInitializer<TTagHelper>
		where TTagHelper : ITagHelper
	{
		/// <summary>
		/// Initializes the <typeparamref name="TTagHelper"/>.
		/// </summary>
		/// <param name="helper">The <typeparamref name="TTagHelper"/> to initialize.</param>
		/// <param name="context">The <see cref="PageContext"/> for the executing view.</param>
		void Initialize(TTagHelper helper, PageContext context);
	}
}
