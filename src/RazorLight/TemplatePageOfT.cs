namespace RazorLight
{
	/// <summary>
	/// Represents the properties and methods that are needed in order to render a template that uses Razor syntax.
	/// </summary>
	/// <typeparam name="TModel">The type of the model.</typeparam>
	public abstract class TemplatePage<TModel> : TemplatePage
	{
		/// <summary>
		/// Gets the Model property.
		/// </summary>
		public TModel Model { get; set; }

		public override void SetModel(object model)
		{
			Model = (TModel)model;
		}
	}
}
