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
        public static void AddRazorLight(this IServiceCollection services, Func<IRazorLightEngine> engineFactoryProvider)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<PropertyInjector>();
            services.AddSingleton<IRazorLightEngine>(p => 
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
