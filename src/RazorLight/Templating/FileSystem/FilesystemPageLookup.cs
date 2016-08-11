using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using RazorLight.Caching;
using RazorLight.Host.Directives;

namespace RazorLight.Templating.FileSystem
{
	public class FilesystemPageLookup : DefaultPageLookup
	{
		public static readonly string ViewExtension = ".cshtml";

		public FilesystemPageLookup(IPageFactoryProvider pageFactoryProvider) : base(pageFactoryProvider)
		{
		}

		protected override IReadOnlyList<PageLookupItem> GetViewStartPages(
			string path,
			HashSet<IChangeToken> expirationTokens)
		{
			var viewStartPages = new List<PageLookupItem>();
			foreach (var viewStartPath in ViewHierarchyUtility.GetViewStartLocations(path))
			{
				PageFactoryResult result = PageFactoryProvider.CreateFactory(viewStartPath);
				if (result.ExpirationTokens != null)
				{
					for (var i = 0; i < result.ExpirationTokens.Count; i++)
					{
						expirationTokens.Add(result.ExpirationTokens[i]);
					}
				}

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

		#region "Helpers"

		public string GetAbsolutePath(string executingFilePath, string pagePath)
		{
			if (string.IsNullOrEmpty(pagePath))
			{
				// Path is not valid; no change required.
				return pagePath;
			}

			if (IsApplicationRelativePath(pagePath))
			{
				// An absolute path already; no change required.
				return pagePath;
			}

			if (!IsRelativePath(pagePath))
			{
				// A page name; no change required.
				return pagePath;
			}

			// Given a relative path i.e. not yet application-relative (starting with "~/" or "/"), interpret
			// path relative to currently-executing view, if any.
			if (string.IsNullOrEmpty(executingFilePath))
			{
				// Not yet executing a view. Start in app root.
				return "/" + pagePath;
			}

			// Get directory name (including final slash) but do not use Path.GetDirectoryName() to preserve path
			// normalization.
			var index = executingFilePath.LastIndexOf('/');
			Debug.Assert(index >= 0);
			return executingFilePath.Substring(0, index + 1) + pagePath;
		}

		private static bool IsApplicationRelativePath(string name)
		{
			Debug.Assert(!string.IsNullOrEmpty(name));
			return name[0] == '~' || name[0] == '/';
		}

		private static bool IsRelativePath(string name)
		{
			Debug.Assert(!string.IsNullOrEmpty(name));

			// Though ./ViewName looks like a relative path, framework searches for that view using view locations.
			return name.EndsWith(ViewExtension, StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}
