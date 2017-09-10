using System;

namespace RazorLight.Caching
{
    public interface ICachingProvider
    {
        TemplateCacheLookupResult GetTemplate(string key);

        void Set(string key, Func<ITemplatePage> pageFactory);

        bool IsTemplateCompiled(string key);
    }
}
