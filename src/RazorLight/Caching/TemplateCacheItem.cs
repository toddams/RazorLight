using System;

namespace RazorLight.Caching
{
	public struct TemplateCacheItem
	{
		/// <summary>
		/// Gets unique template key
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// Gets the <see cref="ITemplatePage"/> factory
		/// </summary>
		public Func<ITemplatePage> TemplatePageFactory { get; set; }

		/// <summary>
		/// Initializes a new instance of <see cref="TemplateCacheItem"/>.
		/// </summary>
		/// <param name="key">The unique key of the <see cref="ITemplatePage"/>.</param>
		/// <param name="pageFactory">The <see cref="ITemplatePage"/> factory.</param>
		public TemplateCacheItem(string key, Func<ITemplatePage> pageFactory)
		{
			Key = key;
			TemplatePageFactory = pageFactory;
		}
	}
}
