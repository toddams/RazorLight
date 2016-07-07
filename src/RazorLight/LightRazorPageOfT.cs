using System;

namespace RazorLight
{
    public abstract class LightRazorPage<TModel> : LightRazorPage
    {
		public TModel Model { get; set; }
	}
}
