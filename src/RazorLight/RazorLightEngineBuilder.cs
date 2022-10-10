using Microsoft.CodeAnalysis;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Generation;
using RazorLight.Razor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace RazorLight
{
	public class RazorLightEngineBuilder
	{
		protected Assembly operatingAssembly;

		protected HashSet<string> namespaces;

		protected ConcurrentDictionary<string, string> dynamicTemplates;

		protected HashSet<MetadataReference> metadataReferences;

		protected HashSet<string> excludedAssemblies;

		protected List<Action<ITemplatePage>> prerenderCallbacks;

		protected RazorLightProject project;

		protected ICachingProvider cachingProvider;

		private bool? disableEncoding;

		private bool? enableDebugMode;

		private RazorLightOptions options;


		/// <summary>
		/// Configures RazorLight to use a project.
		/// </summary>
		/// <remarks>
		/// Use this if implementing a custom <see cref="RazorLightProject"/>.
		/// </remarks>
		/// <param name="razorLightProject"></param>
		/// <returns></returns>
		public virtual RazorLightEngineBuilder UseProject(RazorLightProject razorLightProject)
		{
			project = razorLightProject ?? throw new ArgumentNullException(nameof(razorLightProject), $"Use {nameof(NoRazorProject)} instead of null.  See also {nameof(UseNoProject)}.");

			return this;
		}

		/// <summary>
		/// Configures RazorLight to use a project whose persistent store is a "null device".
		/// </summary>
		public RazorLightEngineBuilder UseNoProject()
		{
			project = new NoRazorProject();

			return this;
		}

		/// <summary>
		/// Configures RazorLight to use a project whose persistent store is the file system.
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public RazorLightEngineBuilder UseFileSystemProject(string root)
		{
			project = new FileSystemRazorProject(root);

			return this;
		}

		/// <summary>
		/// Configures RazorLight to use a project whose persistent store is the file system.
		/// </summary>
		/// <param name="root">Directory path to the root folder containing your Razor markup files.</param>
		/// <param name="extension">If you wish, you can use a different extension than .cshtml.</param>
		/// <returns><see cref="RazorLightEngineBuilder"/></returns>
		public RazorLightEngineBuilder UseFileSystemProject(string root, string extension)
		{
			project = new FileSystemRazorProject(root, extension);

			return this;
		}

		/// <summary>
		/// Configures RazorLight to use a project whose persistent store an assembly manifest resource stream.
		/// </summary>
		/// <param name="rootType">Any type in the root namespace (prefix) for your assembly manifest resource stream.</param>
		/// <returns><see cref="EmbeddedRazorProject"/></returns>
		public RazorLightEngineBuilder UseEmbeddedResourcesProject(Type rootType)
		{
			if (rootType == null) throw new ArgumentNullException(nameof(rootType));

			project = new EmbeddedRazorProject(rootType);

			return this;
		}

		/// <summary>
		/// Configures RazorLight to use a project whose persistent store an assembly manifest resource stream.
		/// </summary>
		/// <param name="assembly">Assembly containing embedded resources</param>
		/// <param name="rootNamespace">The root namespace (prefix) for your assembly manifest resource stream.</param>
		/// <returns></returns>
		public RazorLightEngineBuilder UseEmbeddedResourcesProject(Assembly assembly, string rootNamespace = null)
		{
			project = new EmbeddedRazorProject(assembly, rootNamespace);

			return this;
		}

		public RazorLightEngineBuilder UseOptions(RazorLightOptions razorLightOptions)
		{
			options = razorLightOptions;
			return this;
		}

		/// <summary>
		/// Disables encoding of HTML entities in variables.
		/// </summary>
		/// <example>
		/// The model contains a property with value "&gt;hello&lt;".
		/// 
		/// In the rendered template this will be:
		/// 
		/// <code>
		/// &gt;hello&lt;
		/// </code>
		/// </example>
		/// <returns>A <see cref="RazorLightEngineBuilder"/></returns>
		public RazorLightEngineBuilder DisableEncoding()
		{
			if (disableEncoding.HasValue)
				throw new RazorLightException($"{nameof(disableEncoding)} has already been set");

			disableEncoding = true;
			return this;
		}

		/// <summary>
		/// Enables encoding of HTML entities in variables.
		/// </summary>
		/// <example>
		/// The model contains a property with value "&gt;hello&lt;".
		/// 
		/// In the rendered template this will be:
		/// 
		/// <code>
		/// &amp;gt;hello&amp;lt;
		/// </code>
		/// </example>
		/// <returns>A <see cref="RazorLightEngineBuilder"/></returns>
		public RazorLightEngineBuilder EnableEncoding()
		{
			if (disableEncoding.HasValue)
				throw new RazorLightException($"{nameof(disableEncoding)} has already been set");
			disableEncoding = false;
			return this;
		}

		public virtual RazorLightEngineBuilder UseMemoryCachingProvider()
		{
			cachingProvider = new MemoryCachingProvider();

			return this;
		}

		public virtual RazorLightEngineBuilder UseCachingProvider(ICachingProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException(nameof(provider));
			}

			cachingProvider = provider;

			return this;
		}

		public virtual RazorLightEngineBuilder AddDefaultNamespaces(params string[] namespaces)
		{
			if (namespaces == null)
			{
				throw new ArgumentNullException(nameof(namespaces));
			}

			this.namespaces = new HashSet<string>();

			foreach (string @namespace in namespaces)
			{
				this.namespaces.Add(@namespace);
			}

			return this;
		}

		public virtual RazorLightEngineBuilder AddMetadataReferences(params MetadataReference[] metadata)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException(nameof(metadata));
			}

			metadataReferences = new HashSet<MetadataReference>();

			foreach (var reference in metadata)
			{
				metadataReferences.Add(reference);
			}

			return this;
		}

		public virtual RazorLightEngineBuilder ExcludeAssemblies(params string[] assemblyNames)
		{
			if (assemblyNames == null)
			{
				throw new ArgumentNullException(nameof(assemblyNames));
			}

			excludedAssemblies = new HashSet<string>();

			foreach (var assemblyName in assemblyNames)
			{
				excludedAssemblies.Add(assemblyName);
			}

			return this;
		}
		public virtual RazorLightEngineBuilder AddPrerenderCallbacks(params Action<ITemplatePage>[] callbacks)
		{
			if (callbacks == null)
			{
				throw new ArgumentNullException(nameof(callbacks));
			}

			prerenderCallbacks = new List<Action<ITemplatePage>>();
			prerenderCallbacks.AddRange(callbacks);

			return this;
		}

		public virtual RazorLightEngineBuilder AddDynamicTemplates(IDictionary<string, string> dynamicTemplates)
		{
			if (dynamicTemplates == null)
			{
				throw new ArgumentNullException(nameof(dynamicTemplates));
			}

			this.dynamicTemplates = new ConcurrentDictionary<string, string>(dynamicTemplates);

			return this;
		}

		public virtual RazorLightEngineBuilder SetOperatingAssembly(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			operatingAssembly = assembly;

			return this;
		}

		public virtual RazorLightEngineBuilder EnableDebugMode(bool enableDebugMode = true)
		{
			this.enableDebugMode = enableDebugMode;
			return this;
		}

		public virtual RazorLightEngine Build()
		{
			options = options ?? new RazorLightOptions();
			project = project ?? new NoRazorProject();

			if (namespaces != null)
			{
				if(namespaces.Count > 0 && options.Namespaces.Count > 0)
					ThrowIfHasBeenSetExplicitly(nameof(namespaces));
				
				options.Namespaces = namespaces;
			}

			if (dynamicTemplates != null)
			{
				if(dynamicTemplates.Count > 0 && options.DynamicTemplates.Count > 0)
					ThrowIfHasBeenSetExplicitly(nameof(dynamicTemplates));

				options.DynamicTemplates = dynamicTemplates;
			}

			if (metadataReferences != null)
			{
				if (metadataReferences.Count > 0 && options.AdditionalMetadataReferences.Count > 0)
					ThrowIfHasBeenSetExplicitly(nameof(metadataReferences));

				options.AdditionalMetadataReferences = metadataReferences;
			}

			if (excludedAssemblies != null)
			{
				if(excludedAssemblies.Count > 0 && options.ExcludedAssemblies.Count > 0)
					ThrowIfHasBeenSetExplicitly(nameof(excludedAssemblies));

				options.ExcludedAssemblies = excludedAssemblies;
			}

			if (prerenderCallbacks != null)
			{
				if(prerenderCallbacks.Count > 0 && options.PreRenderCallbacks.Count > 0)
					ThrowIfHasBeenSetExplicitly(nameof(prerenderCallbacks));

				options.PreRenderCallbacks = prerenderCallbacks;
			}

			if (cachingProvider != null)
			{
				if(options.CachingProvider != null)
					ThrowIfHasBeenSetExplicitly(nameof(cachingProvider));

				options.CachingProvider = cachingProvider;
			}

			if (disableEncoding.HasValue)
			{
				if(options.DisableEncoding != null)
					ThrowIfHasBeenSetExplicitly(nameof(disableEncoding));

				options.DisableEncoding = options.DisableEncoding ?? disableEncoding ?? false;
			}
			else
			{
				if (!options.DisableEncoding.HasValue)
					options.DisableEncoding = false;
			}

			if (enableDebugMode.HasValue && options.EnableDebugMode.HasValue)
			{
				ThrowIfHasBeenSetExplicitly(nameof(enableDebugMode));
			}
			else
			{
				options.EnableDebugMode = options.EnableDebugMode ?? enableDebugMode ?? false;
			}

			var metadataReferenceManager = new DefaultMetadataReferenceManager(options.AdditionalMetadataReferences, options.ExcludedAssemblies);
			var assembly = operatingAssembly ?? Assembly.GetEntryAssembly();
			var compiler = new RoslynCompilationService(metadataReferenceManager, assembly, cachingProvider as IPrecompileCallback);

			var sourceGenerator = new RazorSourceGenerator(DefaultRazorEngine.Instance, project, options.Namespaces);
			var templateCompiler = new RazorTemplateCompiler(sourceGenerator, compiler, project, options);
			var templateFactoryProvider = new TemplateFactoryProvider();

			var engineHandler = new EngineHandler(options, templateCompiler, templateFactoryProvider, cachingProvider);

			return new RazorLightEngine(engineHandler);
		}

		private void ThrowIfHasBeenSetExplicitly(string option)
		{
			throw new RazorLightException($"{option} has conflicting settings, configure using either fluent configuration or setting an Options object.");
		}
	}
}