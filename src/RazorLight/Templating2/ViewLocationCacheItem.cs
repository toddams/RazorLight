using System;

namespace RazorLight.Templating
{
	/// <summary>
	/// An item in <see cref="ViewLocationCacheResult"/>.
	/// </summary>
	public struct ViewLocationCacheItem
	{
		/// <summary>
		/// Initializes a new instance of <see cref="ViewLocationCacheItem"/>.
		/// </summary>
		/// <param name="razorPageFactory">The <see cref="TemplatePage"/> factory.</param>
		/// <param name="location">The application relative path of the <see cref="TemplatePage"/>.</param>
		public ViewLocationCacheItem(Func<TemplatePage> razorPageFactory, string location)
		{
			PageFactory = razorPageFactory;
			Location = location;
		}

		/// <summary>
		/// Gets the application relative path of the <see cref="TemplatePage"/>
		/// </summary>
		public string Location { get; }

		/// <summary>
		/// Gets the <see cref="TemplatePage"/> factory.
		/// </summary>
		public Func<TemplatePage> PageFactory { get; }
	}
}
