using System;
using RazorLight.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace RazorLight.Templating
{
    public interface IPageLookup
    {
		IMemoryCache ViewLookupCache { get; }

		IPageFactoryProvider PageFactoryProvider { get; }

		PageCacheResult GetPage(string key);
    }
}
