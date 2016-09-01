using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Text;
using RazorLight.Compilation;

namespace RazorLight.MVC.Tests
{
	public class PropertyInjectorTest
    {
		[Fact]
		public void Throws_On_Null_ServiceCollection()
		{
			Assert.Throws<ArgumentNullException>(() => new PropertyInjector(null));
		}

		[Fact]
		public void Ensure_Registered_Properties_Are_Injected()
		{
			var collection = new ServiceCollection();
			string expectedValue = "TestValue";
			collection.AddSingleton(new TestViewModel() { Title = expectedValue });
			var propertyInjector = new PropertyInjector(collection.BuildServiceProvider());

			var builder = new StringBuilder();
			builder.AppendLine("@model object");
			builder.AppendLine("@inject RazorLight.MVC.Tests.TestViewModel test");
			builder.AppendLine("Hello @test");

			IEngineCore engineCore = new EngineCore(new Templating.Embedded.EmbeddedResourceTemplateManager(typeof(PropertyInjectorTest)));
			CompilationResult result = engineCore.CompileSource(new Templating.LoadedTemplateSource(builder.ToString()), new ModelTypeInfo(typeof(object)));

			var page = (TemplatePage)Activator.CreateInstance(result.CompiledType);
			propertyInjector.Inject(page);

			var prop = page.GetType().GetProperty("test").GetValue(page);

			Assert.NotNull(prop);
			Assert.IsAssignableFrom<TestViewModel>(prop);
			Assert.Equal((prop as TestViewModel).Title, expectedValue);
		}
    }

	public class TestViewModel
	{
		public string Title { get; set; }
	}
}
