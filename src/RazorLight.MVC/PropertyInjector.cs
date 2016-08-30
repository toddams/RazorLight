using System;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;
using RazorLight.Host.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace RazorLight.MVC
{
	public class PropertyInjector
    {
		private readonly IServiceProvider services;
		private readonly ConcurrentDictionary<PropertyInfo, FastPropertySetter> _propertyCache;

		public PropertyInjector(IServiceProvider services)
		{
			if(services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			this.services = services;
			this._propertyCache = new ConcurrentDictionary<PropertyInfo, FastPropertySetter>();
		}

		public void Inject(TemplatePage page)
		{
			if(page == null)
			{
				throw new ArgumentNullException(nameof(page));
			}

			PropertyInfo[] properties = page.GetType().GetRuntimeProperties()
			   .Where(p =>
			   {
				   return
					   p.IsDefined(typeof(RazorInjectAttribute)) &&
					   p.GetIndexParameters().Length == 0 &&
					   !p.SetMethod.IsStatic;
			   }).ToArray();

			foreach(var property in properties)
			{
				Type memberType = property.PropertyType;
				object instance = services.GetRequiredService(memberType);

				FastPropertySetter setter = _propertyCache.GetOrAdd(property, new FastPropertySetter(property));
				setter.SetValue(page, instance);
			}
		}
	}
}
