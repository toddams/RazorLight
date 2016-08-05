using System.Collections.Generic;
using RazorLight.Templating;

namespace RazorLight.Caching
{
    public class PageCacheResult
    {
	    public PageCacheResult()
	    {
		    this.Success = false;
	    }

	    public PageCacheResult(PageCacheItem item, IReadOnlyList<PageCacheItem> viewStartEntries)
	    {
		    this.ViewEntry = item;
		    this.ViewStartEntries = viewStartEntries;
		    this.Success = true;
	    }

		public bool Success { get; }

		public PageCacheItem ViewEntry { get; }

		public IReadOnlyList<PageCacheItem> ViewStartEntries { get; }
	}
}
