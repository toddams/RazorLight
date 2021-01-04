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
using System.Threading.Tasks;
using System.Dynamic;
using System.IO;
using RazorLight.Razor;
using RazorLight.Tests.Utils;

namespace RazorLight.Tests.Extensions
{
	public class ServiceCollectionExtensionsTest
	{
		private readonly string _rootPath = DirectoryUtils.RootDirectory;
		private readonly string _contentRootPath = PathUtility.GetViewsPath();

		private IServiceCollection GetServices()
		{
			var services = new ServiceCollection();
			var envMock = new Mock<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
			envMock.Setup(m => m.ContentRootPath).Returns(_contentRootPath);
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
				return new RazorLightEngineBuilder()
#if NETFRAMEWORK
					.SetOperatingAssembly(typeof(Root).Assembly)
#endif
					.UseEmbeddedResourcesProject(typeof(Root).Assembly).Build();
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

#if !(NETCOREAPP2_0 || NETFRAMEWORK)
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

		[Fact]
		public void Ensure_Works_With_Generic_Host_When_Resolving_IEngineHandler_Before_IRazorLightEngine()
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
			var exception = Assert.Throws<InvalidOperationException>(() => host.Services.GetService<IEngineHandler>());
			Assert.Equal("This exception can only occur if you inject IEngineHandler directly using ServiceCollectionExtensions.AddRazorLight", exception.Message);
			host.Services.GetService<IRazorLightEngine>();
		}

		[Fact()]
		public void Ensure_Works_With_Generic_Host_and_DefaultServiceProvider()
		{
			static IHostBuilder CreateHostBuilder(string[] args)
			{
				return Host.CreateDefaultBuilder(args)
					.UseDefaultServiceProvider((context, options) =>
					{
						options.ValidateScopes = false;
						options.ValidateOnBuild = false;
					})
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

		[Fact()]
		public void Ensure_Works_With_Generic_Host_and_DefaultServiceProvider_ValidateScopes_ValidateOnBuild()
		{
			static IHostBuilder CreateHostBuilder(string[] args)
			{
				return Host.CreateDefaultBuilder(args)
					.UseDefaultServiceProvider((context, options) =>
					{
						options.ValidateScopes = true;
						options.ValidateOnBuild = true;
					})
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

		[Fact()]
		public void Ensure_Works_With_Generic_Host_and_DefaultServiceProvider_ValidateOnBuild()
		{
			static IHostBuilder CreateHostBuilder(string[] args)
			{
				return Host.CreateDefaultBuilder(args)
					.UseDefaultServiceProvider((context, options) =>
					{
						options.ValidateScopes = false;
						options.ValidateOnBuild = true;
					})
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

		[Fact()]
		public void Ensure_Works_With_Generic_Host_and_DefaultServiceProvider_ValidateScopes()
		{
			static IHostBuilder CreateHostBuilder(string[] args)
			{
				return Host.CreateDefaultBuilder(args)
					.UseDefaultServiceProvider((context, options) =>
					{
						options.ValidateScopes = true;
						options.ValidateOnBuild = false;
					})
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
		public void Ensure_DI_Extension_Can_Inject()
		{
			var services = GetServices();
			bool newRazorLightEngineCalled = false;

			services.AddRazorLight()
				.UseMemoryCachingProvider()
				.UseFileSystemProject(_rootPath)
				.UseNetFrameworkLegacyFix();

			services.RemoveAll<IMetadataReferenceManager>();
			services.AddSingleton<IMetadataReferenceManager>(new TestMetadataReferenceManager(() =>
			{
			}));

			services.RemoveAll<IRazorLightEngine>();
			services.AddSingleton<IRazorLightEngine>(new TestRazorLightEngine(() =>
			{
				newRazorLightEngineCalled = true;
			}));

			var provider = services.BuildServiceProvider();
			var directoryFormatter = provider.GetService<IAssemblyDirectoryFormatter>();
			Assert.IsType<LegacyFixAssemblyDirectoryFormatter>(directoryFormatter);

			var project = provider.GetService<RazorLightProject>();
			Assert.IsType<FileSystemRazorProject>(project);
			var fileSystemProject = project as FileSystemRazorProject;
			Assert.Equal(fileSystemProject.Root, _rootPath);

			var engine = provider.GetService<IRazorLightEngine>();
			Assert.NotNull(engine);
			Assert.IsType<TestRazorLightEngine>(engine);
			engine.CompileRenderStringAsync("","","").GetAwaiter().GetResult();
			Assert.True(newRazorLightEngineCalled); 
		
			Assert.IsType<TestMetadataReferenceManager>(provider.GetService<IMetadataReferenceManager>());
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
				_resolveAction();
				return new List<MetadataReference>();
			}
		}

		public class TestRazorLightEngine : IRazorLightEngine
		{

			private Action _compileAction = null;
			public TestRazorLightEngine(Action compileAction)
			{
				_compileAction = compileAction;
			}

			public RazorLightOptions Options => new RazorLightOptions();

			public IEngineHandler Handler => throw new NotImplementedException();

			public Task<string> CompileRenderAsync<T>(string key, T model, ExpandoObject viewBag = null)
			{
				throw new NotImplementedException();
			}

			public Task<string> CompileRenderAsync(string key, object model, Type modelType, ExpandoObject viewBag = null)
			{
				throw new NotImplementedException();
			}

			public Task<string> CompileRenderStringAsync<T>(string key, string content, T model, ExpandoObject viewBag = null)
			{
				_compileAction();
				var result = nameof(TestRazorLightEngine);
				return Task.FromResult(result);
			}

			public Task<ITemplatePage> CompileTemplateAsync(string key)
			{
				throw new NotImplementedException();
			}

			public Task<string> RenderTemplateAsync(ITemplatePage templatePage, object model, Type modelType, ExpandoObject viewBag = null)
			{
				throw new NotImplementedException();
			}

			public Task<string> RenderTemplateAsync<T>(ITemplatePage templatePage, T model, ExpandoObject viewBag = null)
			{
				throw new NotImplementedException();
			}

			public Task RenderTemplateAsync(ITemplatePage templatePage, object model, Type modelType, TextWriter textWriter, ExpandoObject viewBag = null)
			{
				throw new NotImplementedException();
			}
		}

		[Fact]
		public void Try_Render_With_DI_Extension()
		{
			var path = Path.GetDirectoryName(DirectoryUtils.RootDirectory);

			var services = GetServices();
			services.AddRazorLight()
				.UseMemoryCachingProvider()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"));

			var provider = services.BuildServiceProvider();
			var engine = provider.GetService<IRazorLightEngine>();
			var result = engine.CompileRenderAsync<object>("template1.cshtml", null).GetAwaiter().GetResult();
		}
	}
}
