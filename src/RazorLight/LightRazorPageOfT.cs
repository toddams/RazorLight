using System;

namespace RazorLight
{
	/// <summary>
	/// Razor page with a Model
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
    public abstract class LightRazorPage<TModel> : LightRazorPage
    {
		public TModel Model { get; set; }
	}
}
