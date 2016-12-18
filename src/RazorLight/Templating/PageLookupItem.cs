using System;

namespace RazorLight.Templating
{
	public class PageLookupItem
	{
		public string Key { get; set; }

		public Func<ITemplatePage> PageFactory { get; set; }

		public PageLookupItem(string key, Func<ITemplatePage> factory)
		{
			this.Key = key;
			this.PageFactory = factory;
		}
	}
}
