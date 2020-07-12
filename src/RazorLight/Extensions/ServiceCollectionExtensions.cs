using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RazorLight.DependencyInjection;

namespace RazorLight.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static void AddRazorLight(this IServiceCollection services, Func<IRazorLightEngine> engineFactoryProvider)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (engineFactoryProvider == null)
			{
				throw new ArgumentNullException(nameof(engineFactoryProvider));
			}

			services.AddSingleton<PropertyInjector>();
			services.TryAddSingleton<IEngineHandler, EngineHandler>();
			services.TryAddSingleton<IRazorLightEngine>(p =>
			{
				var engine = engineFactoryProvider();
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
