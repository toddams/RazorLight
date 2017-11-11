using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using RazorLight.Extensions;
using System;
using RazorLight.Razor;
using System.IO;

namespace RazorLight.Tests.DependencyInjection
{
	public class ServiceCollectionExtensionsTest
    {
		private string rootPath = PathUtility.GetViewsPath();

		private IServiceCollection GetServices()
		{
			var services = new ServiceCollection();
			var envMock = new Mock<IHostingEnvironment>();
			envMock.Setup(m => m.ContentRootPath).Returns(rootPath);
			services.AddSingleton<IHostingEnvironment>(envMock.Object);

			return services;
		}

		[Fact]
		public void Ensure_Configuration_Is_Applied_On_Embedded()
		{
			var services = GetServices();

			services.AddRazorLight(typeof(Root), o => o.Namespaces.Add("System.Diagnostics"));

			var provider = services.BuildServiceProvider();
			var engine = provider.GetRequiredService<IRazorLightEngine>();

			Assert.NotNull(engine);
			Assert.True(engine.Options.Namespaces.Contains("System.Diagnostics"));
		}

		[Fact]
		public void Ensure_Configuration_Is_Applied_On_Physical()
		{
			var services = GetServices();

			services.AddRazorLight("/", o => o.Namespaces.Add("System.Diagnostics"));

			var provider = services.BuildServiceProvider();
			var engine = provider.GetRequiredService<IRazorLightEngine>();

			Assert.NotNull(engine);
			Assert.True(engine.Options.Namespaces.Contains("System.Diagnostics"));
		}

		[Fact]
		public void AddPhysicall_Allow_Null_OptionsAction()
		{
			var services = GetServices();

			services.AddRazorLight(typeof(Root), null);

			var engine = services.BuildServiceProvider().GetRequiredService<IRazorLightEngine>();

			Assert.NotNull(engine);
			Assert.NotNull(engine.Options);
		}

		[Fact]
		public void AddEmbedded_Allow_Null_OptionsAction()
		{
			var services = GetServices();

			services.AddRazorLight("/", null);

			var engine = services.BuildServiceProvider().GetRequiredService<IRazorLightEngine>();

			Assert.NotNull(engine);
			Assert.NotNull(engine.Options);
		}

		[Fact]
		public void Throws_OnNull_Root()
		{
			var services = GetServices();

			Action action = () => services.AddRazorLight(root: null);

			Assert.Throws<ArgumentNullException>(action);
		}

		[Fact]
		public void Throws_OnNull_RootType()
		{
			var services = GetServices();

			Action action = () => services.AddRazorLight(rootType: null);

			Assert.Throws<ArgumentNullException>(action);
		}

		[Fact]
		public void RootPath_Is_Combined_With_Environment_ContentRootPath()
		{
			var services = GetServices();

			// Assets/
			string environmentContentRootPath = rootPath;
			string givenViewsPath = "Files/Subfolder";

			services.AddRazorLight(givenViewsPath);

			var engine = services.BuildServiceProvider().GetRequiredService<IRazorLightEngine>();
			var project = (FileSystemRazorProject)engine.TemplateFactoryProvider.SourceGenerator.Project;

			string expectedPath = Path.Combine(environmentContentRootPath, givenViewsPath);

			Assert.Equal(expectedPath, project.Root);
		}
	}
}
