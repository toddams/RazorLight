using System;

namespace RazorLight.Templating
{
	public class PageLookupItem
	{
		public string Key { get; set; }

		public Func<TemplatePage> PageFactory { get; set; }

		public PageLookupItem(string key, Func<TemplatePage> factory)
		{
			this.Key = key;
			this.PageFactory = factory;
		}
	}
}
