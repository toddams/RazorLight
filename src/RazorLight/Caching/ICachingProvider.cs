using System;

namespace RazorLight.Caching
{
    public interface ICachingProvider
    {
        TemplateCacheLookupResult GetTemplate(string key);

        void SetTemplate(string key, Func<ITemplatePage> pageFactory);

        bool IsTemplateCompiled(string key);

        void Remove(string key);
    }
}
