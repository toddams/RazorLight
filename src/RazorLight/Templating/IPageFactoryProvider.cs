using System;

namespace RazorLight.Templating
{
	public interface IPageFactoryProvider
	{
		PageFactoryResult CreateFactory(string key);
	}
}
