using System.Collections.Generic;
using RazorLight.Host.Directives;

namespace RazorLight.Templating.FileSystem
{
	public class FilesystemPageLookup : DefaultPageLookup
	{
		public static readonly string ViewExtension = ".cshtml";

		public FilesystemPageLookup(IPageFactoryProvider pageFactoryProvider) : base(pageFactoryProvider)
		{
		}

		protected override IReadOnlyList<PageLookupItem> GetViewStartPages(string path)
		{
			var viewStartPages = new List<PageLookupItem>();
			foreach (var viewStartPath in ViewHierarchyUtility.GetViewStartLocations(path))
			{
				PageFactoryResult result = PageFactoryProvider.CreateFactory(viewStartPath);

				if (result.Success)
				{
					// Populate the viewStartPages list so that _ViewStarts appear in the order the need to be
					// executed (closest last, furthest first). This is the reverse order in which
					// ViewHierarchyUtility.GetViewStartLocations returns _ViewStarts.
					viewStartPages.Insert(0, new PageLookupItem(viewStartPath, result.PageFactory));
				}
			}

			return viewStartPages;
		}
	}
}
