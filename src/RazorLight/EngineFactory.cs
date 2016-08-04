using System;
using RazorLight.Caching;
using RazorLight.Templating;

namespace RazorLight
{
	public static class EngineFactory
	{
		public static RazorLightEngine CreatePhysical(string root)
		{
			if (string.IsNullOrEmpty(root))
			{
				throw new ArgumentNullException(nameof(root));
			}

			var manager = new FilesystemTemplateManager(root);
			var compilerCache = new TrackingCompilerCache(root);
			var core = new EngineCore(manager, compilerCache, EngineConfiguration.Default);

			var pageFactory = new DefaultPageFactory(core.KeyCompile, compilerCache);
			var pageLookup = new FilesystemPageLookup(pageFactory);

			return new RazorLightEngine(core, pageLookup);
		}
	}
}
