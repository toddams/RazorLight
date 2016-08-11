using RazorLight.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace RazorLight.Templating
{
    public interface IPageLookup
    {
		IMemoryCache ViewLookupCache { get; }

		IPageFactoryProvider PageFactoryProvider { get; }

		PageLookupResult GetPage(string key);
    }
}
