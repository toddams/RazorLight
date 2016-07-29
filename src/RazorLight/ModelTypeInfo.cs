using System;
using System.Dynamic;
using RazorLight.Extensions;

namespace RazorLight
{
	public class ModelTypeInfo
	{
		public bool IsStrongType { get; private set; }
		public Type Type { get; private set; }
		public Type TemplateType { get; private set; }
		public string TemplateTypeName { get; private set; }

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
			this.TemplateTypeName = IsStrongType ? Type.Name : "dynamic";
		}
	}
}
