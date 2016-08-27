using System;

namespace RazorLight
{
	/// <summary>
	/// Razor page with a Model
	/// </summary>
	/// <typeparam name="TModel"></typeparam>
	public abstract class TemplatePage<TModel> : TemplatePage
	{
		private object model;

		public TModel Model
		{
			get { return (TModel)model; }
			set { model = value; }
		}

		public override void SetModel(object data)
		{
			Model = (TModel)data;
		}
	}
}
