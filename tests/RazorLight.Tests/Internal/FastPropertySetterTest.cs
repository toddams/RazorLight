using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RazorLight.Internal;
using Xunit;

namespace RazorLight.Tests.Internal
{
	public class TestClass
	{
		public int ValueProperty { get; set; } = 10;

		public string RefProperty { get; set; } = "Hello";
	}


	public class FastPropertySetterTest
	{
		private readonly TestClass testClass = new TestClass();
		private readonly PropertyInfo testPropertyInfo;

		public FastPropertySetterTest()
		{
			testPropertyInfo = testClass.GetType().GetTypeInfo().GetProperty("RefProperty");
		}

		[Fact]
		public void Constructor_Initialized_Main_Properties()
		{
			var setter = new FastPropertySetter(testPropertyInfo);

			Assert.Equal(setter.Property, testPropertyInfo);
			Assert.Equal(setter.Name, testPropertyInfo.Name);
		}

		[Fact]
		public void Ensure_ValueSetter_Initialized()
		{
			var setter = new FastPropertySetter(testPropertyInfo);
			var test = new TestClass();

			string value = "Go";
			setter.SetValue(test, value);

			Assert.NotNull(setter.ValueSetter);
			Assert.Equal(test.RefProperty, value);
		}

		[Fact]
		public void Ensure_FastProperty_Creating_Returns_Action()
		{
			var value = FastPropertySetter.MakeFastPropertySetter(testPropertyInfo);

			Assert.NotNull(value);
			Assert.IsType<Action<object, object>>(value);
		}

		[Fact]
		public void Ensure_ValueSetter_Sets_Value()
		{
			var @class = new TestClass();
			string expected = "Razor";

			var setter = new FastPropertySetter(testPropertyInfo);

			setter.SetValue(@class, expected);

			Assert.Equal(expected, @class.RefProperty);
		}
	}
}
