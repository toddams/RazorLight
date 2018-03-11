using System;
using System.Collections.Generic;
using System.Text;

namespace RazorLight.ViewFeatures.Rendering
{
    public interface IUrlHelper
    {
		/// <summary>
		/// Generates a URL with an absolute path for an action method, which contains the action
		/// name, controller name, route values, protocol to use, host name, and fragment specified by
		/// <see cref="UrlActionContext"/>. Generates an absolute URL if <see cref="UrlActionContext.Protocol"/> and
		/// <see cref="UrlActionContext.Host"/> are non-<c>null</c>.
		/// </summary>
		/// <param name="actionContext">The context object for the generated URLs for an action method.</param>
		/// <returns>The generated URL.</returns>
		string Action(UrlActionContext actionContext);
	}
}
