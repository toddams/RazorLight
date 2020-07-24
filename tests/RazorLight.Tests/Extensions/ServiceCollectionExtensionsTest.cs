using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using RazorLight.Extensions;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using RazorLight.Compilation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace RazorLight.Tests.Extensions
{
	public class ServiceCollectionExtensionsTest
	{
		private string rootPath = PathUtility.GetViewsPath();

		private IServiceCollection GetServices()
		{
			var services = new ServiceCollection();
			var envMock = new Mock<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
			envMock.Setup(m => m.ContentRootPath).Returns(rootPath);
			services.AddSingleton<Microsoft.AspNetCore.Hosting.IHostingEnvironment>(envMock.Object);

			return services;
		}

		[Fact]
		public void Throws_On_Null_EngineFactoryProvider()
		{
			var services = GetServices();

			Assert.Throws<ArgumentNullException>(() => { services.AddRazorLight(null); });
		}

		[Fact]
		public void Ensure_FactoryMethod_Is_Called()
		{
			var services = GetServices();
			bool called = false;

			services.AddRazorLight(() =>
			{
				called = true;
				return new RazorLightEngineBuilder().UseEmbeddedResourcesProject(typeof(Root).Assembly).Build();
			});

			var provider = services.BuildServiceProvider();
			var engine = provider.GetService<IRazorLightEngine>();

			Assert.NotNull(engine);
			Assert.IsType<RazorLightEngine>(engine);
			Assert.True(called);
		}

		public class EmbeddedEngineStartup
		{
			public void Configure(IApplicationBuilder app)
			{

			}
			public void ConfigureServices(IServiceCollection services)
			{
				var embeddedEngine = new RazorLightEngineBuilder()
					.UseEmbeddedResourcesProject(typeof(EmbeddedEngineStartup)) // exception without this (or another project type)
					.UseMemoryCachingProvider()
					.Build();

				services.AddRazorLight(() => embeddedEngine);
			}
		}

#if !(NETCOREAPP2_0)
		[Fact]
		public void Ensure_Works_With_Generic_Host()
		{
			static IHostBuilder CreateHostBuilder(string[] args)
			{
				return Host.CreateDefaultBuilder(args)
					.ConfigureWebHostDefaults(webBuilder =>
					{
						webBuilder.UseStartup<EmbeddedEngineStartup>();
					});
			}

			var hostBuilder = CreateHostBuilder(null);

			Assert.NotNull(hostBuilder);
			var host = hostBuilder.Build();
			Assert.NotNull(host);
			host.Services.GetService<IRazorLightEngine>();
		}
#endif

		[Fact]
		public void Ensure_RazorLightEngineWithFileSystemFactory_Is_Called()
		{
			var services = GetServices();
			var called = false;

			services.AddRazorLight(() =>
			{
				called = true;
				return new RazorLightEngineWithFileSystemProjectFactory().Create();
			});

			var provider = services.BuildServiceProvider();
			var engine = provider.GetService<IRazorLightEngine>();

			Assert.NotNull(engine);
			Assert.IsType<RazorLightEngine>(engine);
			Assert.True(called);
		}

		[Fact]
		public void Ensure_AddRazorLight_DI_Extension_works()
		{
			var services = GetServices();
			bool newMetadataCalled = false;

			services.AddRazorLight()
				.UseMemoryCachingProvider()
				.UseFileSystemProject("");

			services.RemoveAll<IMetadataReferenceManager>();
			services.AddSingleton<IMetadataReferenceManager>(new TestMetadataReferenceManager(()=> 
			{
				newMetadataCalled = true;				
			}));

			var provider = services.BuildServiceProvider();
			var engine = provider.GetService<IRazorLightEngine>();			

			Assert.NotNull(engine);
			Assert.IsType<RazorLightEngine>(engine);
			Assert.IsType<TestMetadataReferenceManager>(provider.GetService<IMetadataReferenceManager>());
			Assert.True(newMetadataCalled);
		}

		public class TestMetadataReferenceManager : IMetadataReferenceManager
		{

			private Action _resolveAction = null;
			public TestMetadataReferenceManager(Action resolveAction)
			{
				_resolveAction = resolveAction;
			}

			public HashSet<MetadataReference> AdditionalMetadataReferences {
				get 
				{
					return new HashSet<MetadataReference>();
				} 
			}

			public IReadOnlyList<MetadataReference> Resolve(Assembly assembly)
			{
				return new List<MetadataReference>();
			}
		}
	}
}
