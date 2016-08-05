using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using RazorLight.Caching;

namespace RazorLight.Templating
{
    public class DefaultPageLookup : IPageLookup
	{
		public DefaultPageLookup(IPageFactoryProvider pageFactoryProvider)
		{
			if (pageFactoryProvider == null)
			{
				throw new ArgumentNullException(nameof(pageFactoryProvider));
			}

			this.PageFactoryProvider = pageFactoryProvider;
		}

		public virtual IMemoryCache ViewLookupCache { get; } = new MemoryCache(new MemoryCacheOptions()
		{
			CompactOnMemoryPressure = false
		});

		public IPageFactoryProvider PageFactoryProvider { get; }

		public virtual TimeSpan CacheExpirationDuration { get; } = TimeSpan.FromMinutes(20);

		public virtual PageCacheResult GetPage(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException(nameof(key));
			}

			PageCacheResult cacheResult;
			if (!ViewLookupCache.TryGetValue(key, out cacheResult))
			{
				var expirationTokens = new HashSet<IChangeToken>();
				cacheResult = CreateCacheResult(expirationTokens, key);

				var cacheEntryOptions = new MemoryCacheEntryOptions();
				cacheEntryOptions.SetSlidingExpiration(CacheExpirationDuration);

				foreach (IChangeToken expirationToken in expirationTokens)
				{
					cacheEntryOptions.AddExpirationToken(expirationToken);
				}

				// No views were found at the specified location. Create a not found result.
				if (cacheResult == null)
				{
					cacheResult = new PageCacheResult();
				}

				cacheResult = ViewLookupCache.Set(
					key,
					cacheResult,
					cacheEntryOptions);
			}

			return cacheResult;
		}

		protected virtual PageCacheResult CreateCacheResult(HashSet<IChangeToken> expirationTokens, string key)
		{
			PageFactoryResult factoryResult = PageFactoryProvider.CreateFactory(key);
			if (factoryResult.ExpirationTokens != null)
			{
				for (var i = 0; i < factoryResult.ExpirationTokens.Count; i++)
				{
					expirationTokens.Add(factoryResult.ExpirationTokens[i]);
				}
			}

			if (factoryResult.Success)
			{
				IReadOnlyList<PageCacheItem> viewStartPages = GetViewStartPages(key, expirationTokens);

				return new PageCacheResult(new PageCacheItem(key, factoryResult.PageFactory), viewStartPages);
			}

			return null;
		}

		protected virtual IReadOnlyList<PageCacheItem> GetViewStartPages(
			string key,
			HashSet<IChangeToken> expirationTokens)
		{
			return new List<PageCacheItem>();
		}

	}
}
