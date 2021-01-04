using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RazorLight.DependencyInjection;
using RazorLight.Tests.Models;
using Xunit;

namespace RazorLight.Tests.DependencyInjection
{
	public class PropertyInjectorTest
	{
		[Fact]
		public void Throws_On_Null_ServiceCollection()
		{
			Assert.Throws<ArgumentNullException>(() => new PropertyInjector(null));
		}

		[Fact]
		public async Task Ensure_Registered_Properties_Are_Injected()
		{
			var collection = new ServiceCollection();
			string expectedValue = "TestValue";
			string templateKey = "key";
			collection.AddSingleton(new TestViewModel() { Title = expectedValue });
			var propertyInjector = new PropertyInjector(collection.BuildServiceProvider());

			var builder = new StringBuilder();
			builder.AppendLine("@model object");
			builder.AppendLine("@inject RazorLight.Tests.Models.TestViewModel test");
			builder.AppendLine("Hello @test");

			var engine = new RazorLightEngineBuilder()
				.UseEmbeddedResourcesProject(typeof(Root))
				.SetOperatingAssembly(typeof(Root).Assembly)
				.AddDynamicTemplates(new Dictionary<string, string>() { { templateKey, builder.ToString() } })
				.Build();

			ITemplatePage templatePage = await engine.CompileTemplateAsync(templateKey);

			//Act
			propertyInjector.Inject(templatePage);

			//Assert
			var prop = templatePage.GetType().GetProperty("test").GetValue(templatePage);

			Assert.NotNull(prop);
			Assert.IsAssignableFrom<TestViewModel>(prop);
			Assert.Equal((prop as TestViewModel).Title, expectedValue);
		}
	}
}