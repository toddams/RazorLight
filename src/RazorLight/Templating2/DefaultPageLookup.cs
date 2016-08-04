//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.Linq;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Primitives;
//using RazorLight.Abstractions;
//using RazorLight.Compilation;
//using RazorLight.Host.Directives;

//namespace RazorLight.Templating
//{
//	public class DefaultPageLookup : IPageLookup
//	{
//		public static readonly string ViewExtension = ".cshtml";
//		private static readonly TimeSpan _cacheExpirationDuration = TimeSpan.FromMinutes(20);
//		private static readonly ViewLocationCacheItem[] EmptyViewStartLocationCacheItems =
//			new ViewLocationCacheItem[0];

//		public DefaultPageLookup(ICompilerCache compilerCache, Func<string, CompilationResult> compileDelegate)
//		{
//			ViewLookupCache = new MemoryCache(new MemoryCacheOptions() { CompactOnMemoryPressure = false });
//			_pageFactory = new DefaultPageFactoryProvider(compileDelegate, compilerCache);
//		}

//		public IMemoryCache ViewLookupCache { get; }

//		private readonly IPageFactoryProvider _pageFactory;

//		public RazorPageResult FindPage(string pageName)
//		{
//			if (string.IsNullOrEmpty(pageName))
//			{
//				throw new ArgumentException(nameof(pageName));
//			}

//			if (IsApplicationRelativePath(pageName) || IsRelativePath(pageName))
//			{
//				// A path; not a name this method can handle.
//				return new RazorPageResult(pageName, Enumerable.Empty<string>());
//			}

//			ViewLocationCacheResult cacheResult = LocatePageFromViewLocations(pageName, isMainPage: false);
//			if (cacheResult.Success)
//			{
//				TemplatePage razorPage = cacheResult.ViewEntry.PageFactory();
//				return new RazorPageResult(pageName, razorPage);
//			}
//			else
//			{
//				return new RazorPageResult(pageName, cacheResult.SearchedLocations);
//			}
//		}

//		public RazorPageResult GetPage(string executingFilePath, string pagePath)
//		{
//			if (string.IsNullOrEmpty(pagePath))
//			{
//				throw new ArgumentException(nameof(pagePath));
//			}

//			if (!(IsApplicationRelativePath(pagePath) || IsRelativePath(pagePath)))
//			{
//				// Not a path this method can handle.
//				return new RazorPageResult(pagePath, Enumerable.Empty<string>());
//			}

//			var cacheResult = LocatePageFromPath(executingFilePath, pagePath, isMainPage: false);
//			if (cacheResult.Success)
//			{
//				var razorPage = cacheResult.ViewEntry.PageFactory();
//				return new RazorPageResult(pagePath, razorPage);
//			}
//			else
//			{
//				return new RazorPageResult(pagePath, cacheResult.SearchedLocations);
//			}
//		}

//		private ViewLocationCacheResult LocatePageFromViewLocations(string pageName, bool isMainPage)
//		{
//			var cacheKey = new ViewLocationCacheKey(pageName, isMainPage);

//			ViewLocationCacheResult cacheResult;
//			if (!ViewLookupCache.TryGetValue(cacheKey, out cacheResult))
//			{
//				cacheResult = OnCacheMiss(cacheKey);
//			}

//			return cacheResult;
//		}

//		private ViewLocationCacheResult LocatePageFromPath(string executingFilePath, string pagePath, bool isMainPage)
//		{
//			string applicationRelativePath = GetAbsolutePath(executingFilePath, pagePath);
//			var cacheKey = new ViewLocationCacheKey(applicationRelativePath, isMainPage);

//			ViewLocationCacheResult cacheResult;
//			if (!ViewLookupCache.TryGetValue(cacheKey, out cacheResult))
//			{
//				var expirationTokens = new HashSet<IChangeToken>();
//				cacheResult = CreateCacheResult(expirationTokens, applicationRelativePath, isMainPage);

//				var cacheEntryOptions = new MemoryCacheEntryOptions();
//				cacheEntryOptions.SetSlidingExpiration(_cacheExpirationDuration);
//				foreach (var expirationToken in expirationTokens)
//				{
//					cacheEntryOptions.AddExpirationToken(expirationToken);
//				}

//				// No views were found at the specified location. Create a not found result.
//				if (cacheResult == null)
//				{
//					cacheResult = new ViewLocationCacheResult(new[] { applicationRelativePath });
//				}

//				cacheResult = ViewLookupCache.Set<ViewLocationCacheResult>(
//					cacheKey,
//					cacheResult,
//					cacheEntryOptions);
//			}

//			return cacheResult;
//		}

//		private ViewLocationCacheResult OnCacheMiss(
//			ViewLocationCacheKey cacheKey)
//		{
//			IEnumerable<string> viewLocations = new List<string>();

//			ViewLocationCacheResult cacheResult = null;
//			var searchedLocations = new List<string>();
//			var expirationTokens = new HashSet<IChangeToken>();
//			foreach (string location in viewLocations)
//			{
//				var path = string.Format(
//					CultureInfo.InvariantCulture,
//					location,
//					cacheKey.ViewName);

//				cacheResult = CreateCacheResult(expirationTokens, path, cacheKey.IsMainPage);
//				if (cacheResult != null)
//				{
//					break;
//				}

//				searchedLocations.Add(path);
//			}

//			// No views were found at the specified location. Create a not found result.
//			if (cacheResult == null)
//			{
//				cacheResult = new ViewLocationCacheResult(searchedLocations);
//			}

//			var cacheEntryOptions = new MemoryCacheEntryOptions();
//			cacheEntryOptions.SetSlidingExpiration(_cacheExpirationDuration);
//			foreach (var expirationToken in expirationTokens)
//			{
//				cacheEntryOptions.AddExpirationToken(expirationToken);
//			}

//			return ViewLookupCache.Set<ViewLocationCacheResult>(cacheKey, cacheResult, cacheEntryOptions);
//		}

//		private ViewLocationCacheResult CreateCacheResult(
//			HashSet<IChangeToken> expirationTokens,
//			string relativePath,
//			bool isMainPage)
//		{
//			RazorPageFactoryResult factoryResult = _pageFactory.CreateFactory(relativePath);
//			if (factoryResult.ExpirationTokens != null)
//			{
//				for (var i = 0; i < factoryResult.ExpirationTokens.Count; i++)
//				{
//					expirationTokens.Add(factoryResult.ExpirationTokens[i]);
//				}
//			}

//			if (factoryResult.Success)
//			{
//				// Only need to lookup _ViewStarts for the main page.
//				var viewStartPages = isMainPage ?
//					GetViewStartPages(relativePath, expirationTokens) :
//					EmptyViewStartLocationCacheItems;

//				return new ViewLocationCacheResult(
//					new ViewLocationCacheItem(factoryResult.RazorPageFactory, relativePath),
//					viewStartPages);
//			}

//			return null;
//		}

//		private IReadOnlyList<ViewLocationCacheItem> GetViewStartPages(
//			string path,
//			HashSet<IChangeToken> expirationTokens)
//		{
//			var viewStartPages = new List<ViewLocationCacheItem>();
//			foreach (var viewStartPath in ViewHierarchyUtility.GetViewStartLocations(path))
//			{
//				var result = _pageFactory.CreateFactory(viewStartPath);
//				if (result.ExpirationTokens != null)
//				{
//					for (var i = 0; i < result.ExpirationTokens.Count; i++)
//					{
//						expirationTokens.Add(result.ExpirationTokens[i]);
//					}
//				}

//				if (result.Success)
//				{
//					// Populate the viewStartPages list so that _ViewStarts appear in the order the need to be
//					// executed (closest last, furthest first). This is the reverse order in which
//					// ViewHierarchyUtility.GetViewStartLocations returns _ViewStarts.
//					viewStartPages.Insert(0, new ViewLocationCacheItem(result.RazorPageFactory, viewStartPath));
//				}
//			}

//			return viewStartPages;
//		}


//		public string GetAbsolutePath(string executingFilePath, string pagePath)
//		{
//			if (string.IsNullOrEmpty(pagePath))
//			{
//				// Path is not valid; no change required.
//				return pagePath;
//			}

//			if (IsApplicationRelativePath(pagePath))
//			{
//				// An absolute path already; no change required.
//				return pagePath;
//			}

//			if (!IsRelativePath(pagePath))
//			{
//				// A page name; no change required.
//				return pagePath;
//			}

//			// Given a relative path i.e. not yet application-relative (starting with "~/" or "/"), interpret
//			// path relative to currently-executing view, if any.
//			if (string.IsNullOrEmpty(executingFilePath))
//			{
//				// Not yet executing a view. Start in app root.
//				return "/" + pagePath;
//			}

//			// Get directory name (including final slash) but do not use Path.GetDirectoryName() to preserve path
//			// normalization.
//			var index = executingFilePath.LastIndexOf('/');
//			Debug.Assert(index >= 0);
//			return executingFilePath.Substring(0, index + 1) + pagePath;
//		}

//		private static bool IsApplicationRelativePath(string name)
//		{
//			Debug.Assert(!string.IsNullOrEmpty(name));
//			return name[0] == '~' || name[0] == '/';
//		}

//		private static bool IsRelativePath(string name)
//		{
//			Debug.Assert(!string.IsNullOrEmpty(name));

//			// Though ./ViewName looks like a relative path, framework searches for that view using view locations.
//			return name.EndsWith(ViewExtension, StringComparison.OrdinalIgnoreCase);
//		}
//	}
//}
