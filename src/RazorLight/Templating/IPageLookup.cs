using System;

namespace RazorLight.Templating
{
	public interface IPageLookup
	{
		IPageFactoryProvider PageFactoryProvider { get; }

		PageLookupResult GetPage(string key);
	}
}
