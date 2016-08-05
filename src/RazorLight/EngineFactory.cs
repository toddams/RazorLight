using System;
using RazorLight.Caching;
using RazorLight.Templating;
using RazorLight.Templating.Embedded;
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

		/// <summary>
		/// Creates a <see cref="RazorLightEngine"/> that resolves templates inside given type assembly as a EmbeddedResource
		/// </summary>
		/// <param name="rootType">Root type where resource is located</param>
		public static RazorLightEngine CreateEmbedded(Type rootType)
		{
			ITemplateManager manager = new EmbeddedResourceTemplateManager(rootType);
			var dependencies = CreateDefaultDependencies(manager);

			return new RazorLightEngine(dependencies.Item1, dependencies.Item2);
		}

		private static Tuple<IEngineCore, IPageLookup> CreateDefaultDependencies(ITemplateManager manager)
		{
			ICompilerCache compilerCache = new DefaultCompilerCache();
			IEngineCore core = new EngineCore(manager, compilerCache, EngineConfiguration.Default);

			IPageFactoryProvider pageFactory = new DefaultPageFactory(core.KeyCompile, compilerCache);
			IPageLookup lookup = new DefaultPageLookup(pageFactory);

			return new Tuple<IEngineCore, IPageLookup>(core, lookup);
		}
	}
}
