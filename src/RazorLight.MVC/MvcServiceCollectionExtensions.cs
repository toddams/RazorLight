using System;
using RazorLight.Templating;
using RazorLight.Templating.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using RazorLight.Caching;
using Microsoft.AspNetCore.Hosting;

namespace RazorLight.MVC
{
	public static class MvcServiceCollectionExtensions
    {
		public static void AddRazorLight(this IServiceCollection services, string root, IEngineConfiguration configuration)
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

			IServiceProvider serviceProvider = services.BuildServiceProvider();
			IHostingEnvironment env = serviceProvider.GetService<IHostingEnvironment>();

			string absoluteRootPath = System.IO.Path.Combine(env.ContentRootPath, root);

			services.AddSingleton(p => configuration);
			services.AddTransient<ITemplateManager>(p => new FilesystemTemplateManager(absoluteRootPath));

			services.AddSingleton<IEngineCore, EngineCore>();

			services.AddSingleton<ICompilerCache>(new TrackingCompilerCache(absoluteRootPath));
			services.AddSingleton<IPageFactoryProvider>(p => new CachingPageFactory(
				p.GetService<IEngineCore>().KeyCompile, 
				p.GetService<ICompilerCache>()));
			services.AddSingleton<IPageLookup, FilesystemPageLookup>();

			services.AddSingleton<IRazorLightEngine, RazorLightEngine>();
		}
    }
}
