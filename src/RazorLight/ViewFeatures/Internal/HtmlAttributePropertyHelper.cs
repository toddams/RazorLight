using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RazorLight.Internal;

namespace RazorLight.ViewFeatures.Internal
{
	internal class HtmlAttributePropertyHelper : FastPropertySetter
	{
		private static readonly ConcurrentDictionary<Type, FastPropertySetter[]> ReflectionCache =
			new ConcurrentDictionary<Type, FastPropertySetter[]>();

		public static new FastPropertySetter[] GetProperties(Type type)
		{
			return GetProperties(type, CreateInstance, ReflectionCache);
		}

		private static FastPropertySetter CreateInstance(PropertyInfo property)
		{
			return new HtmlAttributePropertyHelper(property);
		}

		public HtmlAttributePropertyHelper(PropertyInfo property)
			: base(property)
		{
		}

		public override string Name
		{
			get => base.Name;

			protected set
			{
				base.Name = value == null ? null : value.Replace('_', '-');
			}
		}
	}
}
