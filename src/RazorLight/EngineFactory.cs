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
		public static IRazorLightEngine CreatePhysical(string root)
		{
			return CreatePhysical(root, EngineConfiguration.Default);
		}

		/// <summary>
		/// Creates a <see cref="RazorLightEngine"/> that resolves templates by searching 
		/// them on physical storage with a given <see cref="IEngineConfiguration"/>
		/// and tracks file changes with <seealso cref="System.IO.FileSystemWatcher"/>
		/// </summary>
		/// <param name="root">Root folder where views are stored</param>
		/// <param name="configuration">Engine configuration</param>
		public static IRazorLightEngine CreatePhysical(string root, IEngineConfiguration configuration)
		{
			if (string.IsNullOrEmpty(root))
			{
				throw new ArgumentNullException(nameof(root));
			}

			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			ITemplateManager templateManager = new FilesystemTemplateManager(root);
			IEngineCore core = new EngineCore(templateManager, configuration);

			ICompilerCache compilerCache = new TrackingCompilerCache(root);
			IPageFactoryProvider pageFactory = new CachingPageFactory(core.KeyCompile, compilerCache);
			IPageLookup pageLookup = new FilesystemPageLookup(pageFactory);

			return new RazorLightEngine(core, pageLookup);
		}

		/// <summary>
		/// Creates a <see cref="RazorLightEngine"/> that resolves templates inside given type assembly as a EmbeddedResource
		/// </summary>
		/// <param name="rootType">Root type where resource is located</param>
		public static IRazorLightEngine CreateEmbedded(Type rootType)
		{
			return CreateEmbedded(rootType, EngineConfiguration.Default);
		}

		/// <summary>
		/// Creates a <see cref="RazorLightEngine"/> that resolves templates inside given type assembly as a EmbeddedResource
		/// with a given <see cref="IEngineConfiguration"/>
		/// </summary>
		/// <param name="rootType">Root type where resource is located</param>
		/// <param name="configuration">Engine configuration</param>
		public static IRazorLightEngine CreateEmbedded(Type rootType, IEngineConfiguration configuration)
		{
			ITemplateManager manager = new EmbeddedResourceTemplateManager(rootType);
			var dependencies = CreateDefaultDependencies(manager, configuration);

			return new RazorLightEngine(dependencies.Item1, dependencies.Item2);
		}

		private static Tuple<IEngineCore, IPageLookup> CreateDefaultDependencies(ITemplateManager manager)
		{
			return CreateDefaultDependencies(manager, EngineConfiguration.Default);
		}

		private static Tuple<IEngineCore, IPageLookup> CreateDefaultDependencies(
			ITemplateManager manager,
			IEngineConfiguration configuration)
		{
			IEngineCore core = new EngineCore(manager, configuration);

			IPageFactoryProvider pageFactory = new DefaultPageFactory(core.KeyCompile);
			IPageLookup lookup = new DefaultPageLookup(pageFactory);

			return new Tuple<IEngineCore, IPageLookup>(core, lookup);
		}
	}
}
