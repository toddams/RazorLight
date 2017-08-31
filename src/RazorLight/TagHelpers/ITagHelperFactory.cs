using Microsoft.AspNetCore.Razor.TagHelpers;

namespace RazorLight.TagHelpers
{
    /// <summary>
    /// Provides methods to create and initialize tag helpers.
    /// </summary>
    public interface ITagHelperFactory
    {
        /// <summary>
        /// Creates a new tag helper for the specified <paramref name="context"/>.
        /// </summary>
        /// <param name="context"><see cref="ViewContext"/> for the executing view.</param>
        /// <returns>The tag helper.</returns>
        TTagHelper CreateTagHelper<TTagHelper>(PageContext context) where TTagHelper : ITagHelper;
    }
}
