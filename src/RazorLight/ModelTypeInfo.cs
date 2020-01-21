using RazorLight.Extensions;
using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace RazorLight
{
	/// <summary>
	/// Stores information about model of the template page
	/// </summary>
	public class ModelTypeInfo
	{
		/// <summary>
		/// Indicates whether given model is not a dynamic or anonymous object
		/// </summary>
		public bool IsStrongType { get; private set; }

		/// <summary>
		/// Real type of the model
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		/// Type that will be used on compilation of the template.
		/// If <see cref="Type"/> is anonymous or dynamic - <see cref="TemplateType"/> becomes <see cref="ExpandoObject"/>
		/// </summary>
		public Type TemplateType { get; private set; }

		/// <summary>
		/// Name of the type that will be used on compilation of the template
		/// </summary>
		public string TemplateTypeName { get; private set; }

		/// <summary>
		/// Transforms object into template type
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public object CreateTemplateModel(object model)
		{
			return this.IsStrongType ? model : model.ToExpando();
		}

		public ModelTypeInfo(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			this.Type = type;
			this.IsStrongType = type != typeof(ExpandoObject) && !Type.IsAnonymousType();
			this.TemplateType = IsStrongType ? Type : typeof(ExpandoObject);
			this.TemplateTypeName = IsStrongType ? GetFriendlyName(Type) : "dynamic";
		}

		public static string GetFriendlyName(Type type)
		{
			if (IsGenericType(type))
			{
				return type.Namespace + "." + type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments()
																										.Select(x => GetFriendlyName(x)).ToArray()) + ">";
			}
			else
			{
				return $"{type.Namespace}.{type.Name}";
			}
		}

		private static bool IsGenericType(Type type)
		{
			return type.GetTypeInfo().IsGenericType;
		}
	}
}
