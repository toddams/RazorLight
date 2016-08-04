using System;

namespace RazorLight.Caching
{
    public class PageCacheItem
    {
	    public string Key { get; set; }

	    public Func<TemplatePage> PageFactory { get; set; }

	    public PageCacheItem(string key, Func<TemplatePage> factory)
	    {
		    this.Key = key;
		    this.PageFactory = factory;
	    }
    }
}
