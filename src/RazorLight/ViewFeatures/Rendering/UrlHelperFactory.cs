using System;
using System.Collections.Generic;
using System.Text;

namespace RazorLight.ViewFeatures.Rendering
{
	/// <summary>
	/// A default implementation of <see cref="IUrlHelperFactory"/>.
	/// </summary>
	public class UrlHelperFactory : IUrlHelperFactory
	{
		/// <inheritdoc />
		public IUrlHelper GetUrlHelper(PageContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException((nameof(context)));
			}

			// Perf: Create only one UrlHelper per context
			//object value;
			//if (httpContext.Items.TryGetValue(typeof(IUrlHelper), out value) && value is IUrlHelper)
			//{
			//	return (IUrlHelper)value;
			//}

			var urlHelper = new UrlHelper(context);

			return urlHelper;
		}
	}

	public interface IUrlHelperFactory
	{
		IUrlHelper GetUrlHelper(PageContext context);
	}
}
