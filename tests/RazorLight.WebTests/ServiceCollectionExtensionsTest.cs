using Microsoft.Extensions.DependencyInjection;
using Xunit;
using RazorLight.Extensions;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace RazorLight.WebTests
{
    public class ServiceCollectionExtensionsTest
	{
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
				return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
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
				return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
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
	}
}
