using System;

namespace RazorLight.Caching
{
    public interface ICachingProvider
    {
        TemplateCacheLookupResult RetrieveTemplate(string key);

        void CacheTemplate(string key, Func<ITemplatePage> pageFactory);

        bool Contains(string key);

        void Remove(string key);
    }
}
