using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using RazorLight.Templating;
using RazorLight.Templating.FileSystem;
using RazorLight.Caching;
using RazorLight.Templating.Embedded;

namespace RazorLight.MVC
{
	public static class MvcServiceCollectionExtensions
    {
		public static void AddRazorLight(this IServiceCollection services, string root)
		{
			services.AddRazorLight(root, null);
		}

		public static void AddRazorLight(this IServiceCollection services, string root, Action<IEngineConfiguration> config)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (string.IsNullOrEmpty(root))
			{
				throw new ArgumentNullException(nameof(root));
			}

			if (root.StartsWith("/"))
			{
				root = root.Substring(1);
			}
			else if (root.StartsWith("~/"))
			{
				root = root.Substring(2);
			}

			//Setup configuration
			var configuration = EngineConfiguration.Default;
			config?.Invoke(configuration);

			//Resolve root path 
			IServiceProvider serviceProvider = services.BuildServiceProvider();
			IHostingEnvironment env = serviceProvider.GetService<IHostingEnvironment>();
			string absoluteRootPath = System.IO.Path.Combine(env.ContentRootPath, root);

			services.AddTransient<ITemplateManager>(p => new FilesystemTemplateManager(absoluteRootPath));
			services.AddSingleton<ICompilerCache>(new TrackingCompilerCache(absoluteRootPath));

			services.AddSingleton<IEngineConfiguration>(configuration);
			services.AddSingleton<IEngineCore, EngineCore>();

			services.AddSingleton<IPageFactoryProvider>(p => new CachingPageFactory(
				p.GetService<IEngineCore>().KeyCompile, 
				p.GetService<ICompilerCache>()));

			services.AddSingleton<IPageLookup, FilesystemPageLookup>();
			services.AddSingleton<PropertyInjector>();

			services.AddSingleton<IRazorLightEngine, RazorLightEngine>(p => 
			{
				var engine = new RazorLightEngine(
					p.GetRequiredService<IEngineCore>(),
					p.GetRequiredService<IPageLookup>());

				AddEngineRenderCallbacks(engine, p);

				return engine;
			});
		}

		public static void AddRazorLight(this IServiceCollection services, Type rootType)
		{
			services.AddRazorLight(rootType, null);
		}

		public static void AddRazorLight(this IServiceCollection services, Type rootType, Action<IEngineConfiguration> config)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if(rootType == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			var configuration = EngineConfiguration.Default;
			config?.Invoke(configuration);

			services.AddSingleton(p => configuration);
			services.AddTransient<ITemplateManager>(p => new EmbeddedResourceTemplateManager(rootType));
			services.AddSingleton<IEngineCore, EngineCore>();

			services.AddSingleton<IPageFactoryProvider>(p => new DefaultPageFactory(p.GetService<IEngineCore>().KeyCompile));
			services.AddSingleton<IPageLookup, DefaultPageLookup>();

			services.AddSingleton<IRazorLightEngine, RazorLightEngine>();
		}

		private static void AddEngineRenderCallbacks(IRazorLightEngine engine, IServiceProvider services)
		{
			var injector = services.GetRequiredService<PropertyInjector>();

			engine.PreRenderCallbacks.Add(template => injector.Inject(template));
		}
	}
}
