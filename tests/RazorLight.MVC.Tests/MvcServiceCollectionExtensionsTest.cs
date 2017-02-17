using Microsoft.Extensions.DependencyInjection;
using RazorLight.Caching;
using RazorLight.Templating;
using RazorLight.Templating.FileSystem;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Hosting;

namespace RazorLight.MVC.Tests
{
	public class MvcServiceCollectionExtensionsTest
    {
		private string rootPath = PathUtility.GetViewsPath();

		[Fact]
		public void AddEngine_By_PhysicalRoot_Adds_Filesystem_Components()
		{
			var services = GetServices();
			services.AddRazorLight("/");

			var provider = services.BuildServiceProvider();

			var cache = provider.GetService<ICompilerCache>();
			var pageFactory = provider.GetService<IPageFactoryProvider>();
			var pageLookup = provider.GetService<IPageLookup>();

			Assert.NotNull(cache);
			Assert.NotNull(pageFactory);
			Assert.NotNull(pageLookup);

			Assert.IsType<TrackingCompilerCache>(cache);
			Assert.IsType<CachingPageFactory>(pageFactory);
			Assert.IsType<FilesystemPageLookup>(pageLookup);
		}

		[Fact]
		public void AddEngine_By_RootType_Adds_Default_Components()
		{
			var services = GetServices();
			services.AddRazorLight(typeof(Fixtures.TestClass));

			var provider = services.BuildServiceProvider();

			var pageFactory = provider.GetService<IPageFactoryProvider>();
			var pageLookup = provider.GetService<IPageLookup>();

			Assert.NotNull(pageFactory);
			Assert.NotNull(pageLookup);

			Assert.IsType<DefaultPageFactory>(pageFactory);
			Assert.IsType<DefaultPageLookup>(pageLookup);
		}

		[Fact]
		public void Ensure_Configuration_Is_Applied_On_Physical()
		{
			var services = GetServices();

			services.AddRazorLight("/", o => o.Namespaces.Add("System.Diagnostics"));

			var provider = services.BuildServiceProvider();
			var engine = provider.GetRequiredService<IRazorLightEngine>();

			Assert.NotNull(engine);
			Assert.True(engine.Configuration.Namespaces.Contains("System.Diagnostics"));
		}

		[Fact]
		public void Ensure_Configuration_Is_Applied_On_Embedded()
		{
			var services = GetServices();

			services.AddRazorLight(typeof(Fixtures.TestClass), o => o.Namespaces.Add("System.Diagnostics"));

			var provider = services.BuildServiceProvider();
			var engine = provider.GetRequiredService<IRazorLightEngine>();

			Assert.NotNull(engine);
			Assert.True(engine.Configuration.Namespaces.Contains("System.Diagnostics"));
		}

		private IServiceCollection GetServices()
		{
			var services = new ServiceCollection();
			var envMock = new Mock<IHostingEnvironment>();
			envMock.Setup(m => m.ContentRootPath).Returns(rootPath);
			services.AddSingleton<IHostingEnvironment>(envMock.Object);

			return services;
		}
    }
}
