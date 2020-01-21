using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;

namespace RazorLight.Caching
{
	public class MemoryCachingProvider : ICachingProvider
	{
		public MemoryCachingProvider()
		{
			var cacheOptions = Options.Create(new MemoryCacheOptions());
			LookupCache = new MemoryCache(cacheOptions);
		}

		protected IMemoryCache LookupCache { get; set; }

		public TemplateCacheLookupResult RetrieveTemplate(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (LookupCache.TryGetValue(key, out TemplateCacheItem template))
			{
				var result = new TemplateCacheLookupResult(template);

				return result;
			}
			else
			{
				return new TemplateCacheLookupResult();
			}
		}

		public bool Contains(string key)
		{
			return LookupCache.TryGetValue(key, out _);
		}

		public void CacheTemplate(string key, Func<ITemplatePage> pageFactory, IChangeToken expirationToken = null)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (pageFactory == null)
			{
				throw new ArgumentNullException(nameof(pageFactory));
			}

			var cacheEntryOptions = new MemoryCacheEntryOptions();
			if (expirationToken != null)
			{
				cacheEntryOptions.ExpirationTokens.Add(expirationToken);
			}

			var cacheItem = new TemplateCacheItem(key, pageFactory);
			LookupCache.Set(key, cacheItem, cacheEntryOptions);
		}

		public void Remove(string key)
		{
			LookupCache.Remove(key);
		}
	}
}
