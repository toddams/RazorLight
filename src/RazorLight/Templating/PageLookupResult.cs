using System.Collections.Generic;

namespace RazorLight.Templating
{
	public class PageLookupResult
	{
		public static PageLookupResult Failed => new PageLookupResult();

		private PageLookupResult()
		{
			this.Success = false;
		}

		public PageLookupResult(PageLookupItem item, IReadOnlyList<PageLookupItem> viewStartEntries)
		{
			this.ViewEntry = item;
			this.ViewStartEntries = viewStartEntries;
			this.Success = true;
		}

		public bool Success { get; }

		public PageLookupItem ViewEntry { get; }

		public IReadOnlyList<PageLookupItem> ViewStartEntries { get; }
	}
}
