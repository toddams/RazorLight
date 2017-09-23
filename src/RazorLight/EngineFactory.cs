using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Instrumentation;
using RazorLight.Razor;
using System;

namespace RazorLight
{
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
        ///Creates RazorLightEngine with a custom RazorLightProject
        /// </summary>
        /// <param name="project">The project</param>
        /// <returns>Instance of RazorLightEngine</returns>
        public virtual RazorLightEngine Create(RazorLightProject project, RazorLightOptions options = null)
        {
            var razorOptions = options ?? new RazorLightOptions();

            var sourceGenerator = new RazorSourceGenerator(RazorEngine, project);

            var metadataReferenceManager = new DefaultMetadataReferenceManager(razorOptions.AdditionalMetadataReferences);
            var compiler = new RoslynCompilationService(metadataReferenceManager);
            var templateFactoryProvider = new TemplateFactoryProvider(sourceGenerator, compiler, razorOptions);

            ICachingProvider cacheProvider = new DefaultCachingProvider();

            return new RazorLightEngine(razorOptions, templateFactoryProvider, cacheProvider);
        }

        public virtual RazorEngine RazorEngine
        {
            get
            {
                return RazorEngine.Create(builder =>
                {
                    Instrumentation.InjectDirective.Register(builder);
                    Instrumentation.ModelDirective.Register(builder);
                    NamespaceDirective.Register(builder);
                    FunctionsDirective.Register(builder);
                    InheritsDirective.Register(builder);
                    SectionDirective.Register(builder);

                    //builder.AddTargetExtension(new TemplateTargetExtension()
                    //{
                    //    TemplateTypeName = "global::Microsoft.AspNetCore.Mvc.Razor.HelperResult",
                    //});

                    builder.Features.Add(new ModelExpressionPass());
                    //builder.Features.Add(new ViewComponentTagHelperPass());
                    builder.Features.Add(new RazorLightTemplateDocumentClassifierPass());

                    if (!builder.DesignTime)
                    {
                        builder.Features.Add(new RazorLightAssemblyAttributeInjectionPass());
                        builder.Features.Add(new InstrumentationPass());
                    }
                });
            }
        }
    }
}
