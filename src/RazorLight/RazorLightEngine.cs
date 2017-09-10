using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Instrumentation;
using RazorLight.Razor;
using RazorLight.Rendering;
using System;
using System.Dynamic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace RazorLight
{
    public class RazorLightEngine
    {
        private TemplateFactoryProvider templateFactoryProvider;
        private ICachingProvider cache;

        public RazorLightEngine()
        {
            var engine = RazorEngine.Create(builder =>
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

            var project = new FileSystemRazorProject("C:\\");
            var sourceGenerator = new RazorSourceGenerator(engine, project);
            var compiler = new RoslynCompilationService(new DefaultMetadataReferenceManager());

            templateFactoryProvider = new TemplateFactoryProvider(sourceGenerator, compiler, project);
            cache = new DefaultCachingProvider();
        }

        public async Task<string> CompileRenderAsync(string key, object model, Type modelType, ExpandoObject viewBag)
        {
            ITemplatePage template = await GetTemplateAsync(key).ConfigureAwait(false);

            var context = new PageContext(viewBag)
            {
                ExecutingPageKey = key,
                ModelTypeInfo = new ModelTypeInfo(modelType)
            };

            template.PageContext = context;

            return await RunTemplateAsync(template, model).ConfigureAwait(false);
        }

        public async Task<ITemplatePage> GetTemplateAsync(string key, bool compileIfNotCached = true)
        {
            var cacheLookupResult = cache.GetTemplate(key);
            if(cacheLookupResult.Success)
            {
                return cacheLookupResult.Template.TemplatePageFactory();
            }

            if(compileIfNotCached)
            {
                var pageFactoryResult = await templateFactoryProvider.CreateFactoryAsync(key).ConfigureAwait(false);
                if (!pageFactoryResult.Success)
                {
                    throw new Exception($"Template {key} is corrupted or invalid");
                }

                cache.Set(key, pageFactoryResult.TemplatePageFactory);

                return pageFactoryResult.TemplatePageFactory();
            }

            throw new RazorLightException($"Can't find a view with a specified key ({key})");
        }

        public async Task<string> RunTemplateAsync(ITemplatePage templatePage, object model)
        {
            object pageModel = templatePage.PageContext.ModelTypeInfo.CreateTemplateModel(model);
            templatePage.SetModel(pageModel);
            templatePage.Key = templatePage.PageContext.ExecutingPageKey;

            using (var writer = new StringWriter())
            {
                templatePage.PageContext.Writer = writer;

                using (var renderer = new TemplateRenderer(templatePage, this, HtmlEncoder.Default))
                {
                    await renderer.RenderAsync().ConfigureAwait(false);
                }

                return writer.ToString();
            }
        }
    }
}
