using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;

namespace RazorLight.Caching
{
    public class DefaultCachingProvider : ICachingProvider
    {
        public DefaultCachingProvider()
        {
            var cacheOptions = Options.Create(new MemoryCacheOptions());
            LookupCache = new MemoryCache(cacheOptions);
        }

        protected IMemoryCache LookupCache { get; set; }

        public TemplateCacheLookupResult RetrieveTemplate(string key)
        {
            if(LookupCache.TryGetValue(key, out TemplateCacheItem template))
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

        public void CacheTemplate(string key, Func<ITemplatePage> pageFactory)
        {
            var cacheItem = new TemplateCacheItem(key, pageFactory);
            LookupCache.Set(key, cacheItem);
        }

        public void Remove(string key)
        {
            LookupCache.Remove(key);
        }
    }
}
