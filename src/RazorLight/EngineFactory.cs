using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Instrumentation;
using RazorLight.Razor;

namespace RazorLight
{
    public class EngineFactory
    {
        public RazorLightEngine ForFileSystem(string root)
        {
            ICachingProvider cacheProvider = new DefaultCachingProvider();

            var project = new FileSystemRazorProject(root);
            var sourceGenerator = new RazorSourceGenerator(RazorEngine, project);
            var compiler = new RoslynCompilationService(new DefaultMetadataReferenceManager());

            var templateFactoryProvider = new TemplateFactoryProvider(project, sourceGenerator, compiler);

            return new RazorLightEngine(templateFactoryProvider, cacheProvider);
        }

        protected virtual RazorEngine RazorEngine
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
                    //builder.Features.Add(new PagesPropertyInjectionPass());
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
