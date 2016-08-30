using System;
using System.Diagnostics;
using System.Reflection;

namespace RazorLight.MVC
{
	public class Test
	{
		public string Go { get; set; }
	}

	public class FastPropertySetter
    {
		private static readonly MethodInfo CallPropertySetterOpenGenericMethod =
			typeof(FastPropertySetter).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertySetter));

		/// <summary>
		/// Initializes a fast <see cref="FastPropertySetter"/>.
		/// </summary>
		public FastPropertySetter(PropertyInfo property)
		{
			if (property == null)
			{
				throw new ArgumentNullException(nameof(property));
			}

			Property = property;
			Name = property.Name;
			_valueSetter = MakeFastPropertySetter(Property);
		}

		private Action<object, object> _valueSetter;

		/// <summary>
		/// Gets the backing <see cref="PropertyInfo"/>.
		/// </summary>
		public PropertyInfo Property { get; }

		/// <summary>
		/// Gets (or sets in derived types) the property name.
		/// </summary>
		public virtual string Name { get; protected set; }

		/// <summary>
		/// Gets the property value setter.
		/// </summary>
		public Action<object, object> ValueSetter
		{
			get
			{
				if (_valueSetter == null)
				{
					_valueSetter = MakeFastPropertySetter(Property);
				}

				return _valueSetter;
			}
		}

		/// <summary>
		/// Sets the property value for the specified <paramref name="instance" />.
		/// </summary>
		/// <param name="instance">The object whose property value will be set.</param>
		/// <param name="value">The property value.</param>
		public void SetValue(object instance, object value)
		{
			ValueSetter(instance, value);
		}

		/// <summary>
		/// Creates a single fast property setter for reference types. The result is not cached.
		/// </summary>
		/// <param name="propertyInfo">propertyInfo to extract the setter for.</param>
		/// <returns>a fast getter.</returns>
		/// <remarks>
		/// This method is more memory efficient than a dynamically compiled lambda, and about the
		/// same speed. This only works for reference types.
		/// </remarks>
		public static Action<object, object> MakeFastPropertySetter(PropertyInfo propertyInfo)
		{
			if(propertyInfo == null)
			{
				throw new ArgumentNullException(nameof(propertyInfo));
			}

			if (propertyInfo.DeclaringType.GetTypeInfo().IsValueType)
			{
				throw new RazorLightException("Only reference types are allowed");
			}

			MethodInfo setMethod = propertyInfo.SetMethod;
			Debug.Assert(setMethod != null);
			Debug.Assert(!setMethod.IsStatic);
			Debug.Assert(setMethod.ReturnType == typeof(void));

			ParameterInfo[] parameters = setMethod.GetParameters();
			Debug.Assert(parameters.Length == 1);

			// Instance methods in the CLR can be turned into static methods where the first parameter
			// is open over "target". This parameter is always passed by reference, so we have a code
			// path for value types and a code path for reference types.
			Type typeInput = setMethod.DeclaringType;
			Type parameterType = parameters[0].ParameterType;

			// Create a delegate TDeclaringType -> { TDeclaringType.Property = TValue; }
			Delegate propertySetterAsAction =
				setMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(typeInput, parameterType));

			MethodInfo callPropertySetterClosedGenericMethod =
				CallPropertySetterOpenGenericMethod.MakeGenericMethod(typeInput, parameterType);

			Delegate callPropertySetterDelegate =
				callPropertySetterClosedGenericMethod.CreateDelegate(
					typeof(Action<object, object>), propertySetterAsAction);

			return (Action<object, object>)callPropertySetterDelegate;
		}

		private static void CallPropertySetter<TDeclaringType, TValue>(
			Action<TDeclaringType, TValue> setter,
			object target,
			object value)
		{
			setter((TDeclaringType)target, (TValue)value);
		}
	}
}
