using RazorLight.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

namespace RazorLight.Tests
{
	public class TypeExtensionsTest
    {
		[Fact]
		public void ToExpando_Returns_ExpandoObject_If_Passed()
		{
			var expando = new ExpandoObject();

			var obj = expando.ToExpando();

			Console.WriteLine(expando.GetType() == typeof(ExpandoObject));
		}

		[Fact]
		public void ToExpando_Returns_All_Properties_Of_AnonymousObject()
		{
			var obj = new
			{
				Name = "Test",
				Age = 18
			};

			IDictionary<string, object> expando = obj.ToExpando();

			Assert.True(expando.ContainsKey("Name"));
			Assert.True(expando.ContainsKey("Age"));

			Assert.Equal(expando["Name"], "Test");
			Assert.Equal(Convert.ToInt32(expando["Age"]), 18);
		}

		[Fact]
		public void Returns_True_For_Anynymous_Objects()
		{
			var obj = new { Name = "Test" };

			Assert.True(TypeExtensions.IsAnonymousType(obj.GetType()));
		}

		public void Returns_False_For_Strong_Types()
		{
			Assert.False(TypeExtensions.IsAnonymousType(typeof(TestViewModel)));
		}
	}
}
