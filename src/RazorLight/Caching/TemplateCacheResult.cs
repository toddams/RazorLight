using System.Collections.Generic;

namespace RazorLight.Caching
{
    public class TemplateCacheResult
    {
        /// <summary>
        /// <see cref="TemplateCacheItem"/> for the located template.
        /// </summary>
        /// <remarks><c>null</c> if <see cref="Success"/> is <c>false</c>.</remarks>
        public TemplateCacheItem TemplateEntry { get; }

        /// <summary>
        /// <see cref="TemplateCacheItem"/>s for applicable _ViewStarts.
        /// </summary>
        /// <remarks><c>null</c> if <see cref="Success"/> is <c>false</c>.</remarks>
        public IReadOnlyList<TemplateCacheItem> ViewStartEntries { get; }

        /// <summary>
        /// Gets a value that indicates whether the view was successfully found.
        /// </summary>
        public bool Success { get; }
    }
}
