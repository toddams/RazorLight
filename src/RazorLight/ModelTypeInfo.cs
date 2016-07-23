using System;
using System.Dynamic;
using RazorLight.Extensions;

namespace RazorLight
{
	public class ModelTypeInfo
	{
		public Type Type { get; private set; }
		public bool IsAnonymousType { get; private set; }
		public string TemplateTypeName
		{
			get { return IsAnonymousType ? "dynamic" : Type.FullName; }
		}

		public ModelTypeInfo(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			this.Type = type;
			this.IsAnonymousType = type.IsAnonymousType();
		}
	}

    public class ModelTypeInfo<T> : ModelTypeInfo
    {
	    public T Value { get; private set; }

	    public ModelTypeInfo(T value) : base(typeof(T))
	    {
		    if (value == null)
		    {
			    throw new ArgumentNullException(nameof(value));
		    }

		    this.Value = value;
	    }
    }
}
