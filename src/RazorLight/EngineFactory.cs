using System;
using RazorLight.Caching;
using RazorLight.Templating;
using RazorLight.Templating.FileSystem;

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

			ITemplateManager manager = new FilesystemTemplateManager(root);
			ICompilerCache compilerCache = new TrackingCompilerCache(root);
			IEngineCore core = new EngineCore(manager, compilerCache, EngineConfiguration.Default);

			IPageFactoryProvider pageFactory = new DefaultPageFactory(core.KeyCompile, compilerCache);
			IPageLookup pageLookup = new FilesystemPageLookup(pageFactory);

			return new RazorLightEngine(core, pageLookup);
		}
	}
}
