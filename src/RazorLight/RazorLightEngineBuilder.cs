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

        private bool disableEncoding = false;

        public virtual RazorLightEngineBuilder UseProject(RazorLightProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            this.project = project;

            return this;
        }

        public RazorLightEngineBuilder UseEmbeddedResourcesProject(Type rootType)
        {
            project = new EmbeddedRazorProject(rootType);

            return this;
        }


		/// <summary>
		/// Disables encoding of HTML entities in variables.
		/// </summary>
		/// <example>
		/// The model contais a property with value "&gt;hello&lt;".
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
			disableEncoding = true;
			return this;
		}

		/// <summary>
		/// Enables encoding of HTML entities in variables.
		/// </summary>
		/// <example>
		/// The model contais a property with value "&gt;hello&lt;".
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
			disableEncoding = false;
			return this;
		}

		public RazorLightEngineBuilder UseEmbeddedResourcesProject(Assembly assembly, string rootNamespace = null)
		{
			project = new EmbeddedRazorProject(assembly, rootNamespace);

			return this;
		}


		public RazorLightEngineBuilder UseFileSystemProject(string root)
        {
            project = new FileSystemRazorProject(root);

            return this;
        }

        public RazorLightEngineBuilder UseFileSystemProject(string root, string extension)
        {
            project = new FileSystemRazorProject(root, extension);

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
            if(assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            operatingAssembly = assembly;

            return this;
        }

        public virtual RazorLightEngine Build()
        {
            var options = new RazorLightOptions();

            if (namespaces != null)
            {
                options.Namespaces = namespaces;
            }

            if (dynamicTemplates != null)
            {
                options.DynamicTemplates = dynamicTemplates;
            }

            if (metadataReferences != null)
            {
                options.AdditionalMetadataReferences = metadataReferences;
            }

            if (excludedAssemblies != null)
            {
                options.ExcludedAssemblies = excludedAssemblies;
            }

            if (prerenderCallbacks != null)
            {
                options.PreRenderCallbacks = prerenderCallbacks;
            }

			if(cachingProvider != null)
			{
				options.CachingProvider = cachingProvider;
			}

            options.DisableEncoding = disableEncoding;


            var metadataReferenceManager = new DefaultMetadataReferenceManager(options.AdditionalMetadataReferences, options.ExcludedAssemblies);
            var assembly = operatingAssembly ?? Assembly.GetEntryAssembly();
            var compiler = new RoslynCompilationService(metadataReferenceManager, assembly);

			var sourceGenerator = new RazorSourceGenerator(DefaultRazorEngine.Instance, project, options.Namespaces);
			var templateCompiler = new RazorTemplateCompiler(sourceGenerator, compiler, project, options);
			var templateFactoryProvider = new TemplateFactoryProvider();

			var engineHandler = new EngineHandler(options, templateCompiler, templateFactoryProvider, cachingProvider);

            return new RazorLightEngine(engineHandler);
        }
    }
}