using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RazorLight.DependencyInjection;

namespace RazorLight.Extensions
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds RazorLight services that resolve templates from a given <paramref name="root"/>
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="root">Root views folder</param>
		/// <param name="optionsAction">Configuration (can be null)</param>
		public static void AddRazorLight(this IServiceCollection services, string root, Action<RazorLightOptions> optionsAction = null)
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
			var options = new RazorLightOptions();
			optionsAction?.Invoke(options);

			//Resolve root path 
			IServiceProvider serviceProvider = services.BuildServiceProvider();
			IHostingEnvironment env = serviceProvider.GetService<IHostingEnvironment>();
			string absoluteRootPath = System.IO.Path.Combine(env.ContentRootPath, root);
			
			services.AddSingleton<PropertyInjector>();
			services.AddSingleton<IRazorLightEngine, RazorLightEngine>(p =>
			{
				var engine = new EngineFactory().ForFileSystem(absoluteRootPath, options);

				AddEngineRenderCallbacks(engine, p);

				return engine;
			});
		}

		/// <summary>
		/// Creates RazorLight services that resolves templates inside given type assembly as a EmbeddedResource
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="rootType">Root type where</param>
		/// <param name="options">Configuration (can be null)</param>
		public static void AddRazorLight(this IServiceCollection services, Type rootType, Action<RazorLightOptions> optionsAction = null)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (rootType == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			var options = new RazorLightOptions();
			optionsAction?.Invoke(options);

			services.AddSingleton<PropertyInjector>();
			services.AddSingleton<IRazorLightEngine, RazorLightEngine>(p =>
			{
				var engine = new EngineFactory().ForEmbeddedResources(rootType, options);

				AddEngineRenderCallbacks(engine, p);

				return engine;
			});
		}

		private static void AddEngineRenderCallbacks(IRazorLightEngine engine, IServiceProvider services)
		{
			var injector = services.GetRequiredService<PropertyInjector>();

			engine.Options.PreRenderCallbacks.Add(template => injector.Inject(template));
		}
	}
}
