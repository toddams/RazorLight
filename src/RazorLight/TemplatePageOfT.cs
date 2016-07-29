using System;

namespace RazorLight
{
	/// <summary>
	/// Razor page with a Model
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
    public abstract class TemplatePage<TModel> : TemplatePage
    {
		public TModel Model { get; set; }
	}
}
