using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Generation;
using RazorLight.Instrumentation;
using RazorLight.Razor;

namespace RazorLight
{
    [Obsolete("Use RazorLightEngineBuilder instead")]
    public class EngineFactory : IEngineFactory
    {
        /// <summary>
        /// Creates RazorLightEngine with a filesystem razor project
        /// </summary>
        /// <param name="root">Root folder where views are stored</param>
        /// <returns>Instance of RazorLightEngine</returns>
        public virtual RazorLightEngine ForFileSystem(string root)
        {
            var project = new FileSystemRazorProject(root);

            return Create(project);
        }

		/// <summary>
		/// Creates RazorLightEngine with a filesystem razor project
		/// </summary>
		/// <param name="root">Root folder where views are stored</param>
		/// <param name="options">Engine options</param>
		/// <returns>Instance of RazorLightEngine</returns>
		public virtual RazorLightEngine ForFileSystem(string root, RazorLightOptions options)
		{
			var project = new FileSystemRazorProject(root);

			return Create(project, options);
		}

		/// <summary>
		/// Creates RazorLightEngine with a embedded resource razor project
		/// </summary>
		/// <param name="rootType">Type of the root.</param>
		/// <returns>Instance of RazorLightEngine</returns>
		public virtual RazorLightEngine ForEmbeddedResources(Type rootType)
        {
            var project = new EmbeddedRazorProject(rootType);

            return Create(project);
        }

		/// <summary>
		/// Creates RazorLightEngine with a embedded resource razor project
		/// </summary>
		/// <param name="rootType">Type of the root.</param>
		/// <param name="options">Engine options</param>
		/// <returns>Instance of RazorLightEngine</returns>
		public virtual RazorLightEngine ForEmbeddedResources(Type rootType, RazorLightOptions options)
		{
			var project = new EmbeddedRazorProject(rootType);

			return Create(project, options);
		}

		public RazorLightEngine Create(RazorLightOptions options = null)
		{
			return Create(null, options);
		}

		/// <summary>
		///Creates RazorLightEngine with a custom RazorLightProject
		/// </summary>
		/// <param name="project">The project</param>
		/// <returns>Instance of RazorLightEngine</returns>
		public virtual RazorLightEngine Create(RazorLightProject project, RazorLightOptions options = null)
        {
            var razorOptions = options ?? new RazorLightOptions();

            var sourceGenerator = new RazorSourceGenerator(DefaultRazorEngine.Instance, project, razorOptions.Namespaces);

            var metadataReferenceManager = new DefaultMetadataReferenceManager(razorOptions.AdditionalMetadataReferences, razorOptions.ExcludedAssemblies);
            var compiler = new RoslynCompilationService(metadataReferenceManager, Assembly.GetEntryAssembly());
            var templateFactoryProvider = new TemplateFactoryProvider(sourceGenerator, compiler, razorOptions);

            ICachingProvider cacheProvider = new MemoryCachingProvider();

            return new RazorLightEngine(razorOptions, templateFactoryProvider, cacheProvider);
        }
    }
}
