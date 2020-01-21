using Microsoft.Extensions.Primitives;
using System;

namespace RazorLight.Caching
{
	public interface ICachingProvider
	{
		TemplateCacheLookupResult RetrieveTemplate(string key);

		void CacheTemplate(string key, Func<ITemplatePage> pageFactory, IChangeToken expirationToken);

		bool Contains(string key);

		void Remove(string key);
	}
}
