using Microsoft.Extensions.Caching.Memory;
using RazorLight.Templating;

namespace RazorLight.Abstractions
{
    public interface IPageLookup
    {
		IMemoryCache ViewLookupCache { get; }

	    RazorPageResult FindPage(string pageName);

	    RazorPageResult GetPage(string executingFilePath, string pagePath);
    }
}
