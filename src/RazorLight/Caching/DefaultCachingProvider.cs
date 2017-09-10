using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
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

        public TemplateCacheLookupResult GetTemplate(string key)
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

        public bool IsTemplateCompiled(string key)
        {
            return LookupCache.TryGetValue(key, out _);
        }

        public void Set(string key, Func<ITemplatePage> pageFactory)
        {
            var cacheItem = new TemplateCacheItem(key, pageFactory);
            LookupCache.Set(key, cacheItem);
        }
    }
}
