using System;
using RazorLight.Caching;
using RazorLight.Templating;
using RazorLight.Templating.FileSystem;

namespace RazorLight
{
	public static class EngineFactory
	{
		/// <summary>
		/// Creates a <see cref="RazorLightEngine"/> that resolves templates by searching them on physical storage
		/// and tracks file changes with <seealso cref="System.IO.FileSystemWatcher"/>
		/// </summary>
		/// <param name="root">Root folder where views are stored</param>
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
