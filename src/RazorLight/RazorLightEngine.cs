using RazorLight.Caching;
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
        private ITemplateFactoryProvider templateFactoryProvider;
        private ICachingProvider cache;

        public RazorLightEngine(
            ITemplateFactoryProvider factoryProvider,
            ICachingProvider cachingProvider)
        {
            templateFactoryProvider = factoryProvider;
            cache = cachingProvider;
        }

        /// <summary>
        /// Compiles and renders a template with a given <paramref name="key"/>
        /// </summary>
        /// <typeparam name="T">Type of the model</typeparam>
        /// <param name="key">Unique key of the template</param>
        /// <param name="model">Template model</param>
        /// <returns>Rendered template as a string result</returns>
        public async Task<string> CompileRenderAsync<T>(string key, T model)
        {
            return await CompileRenderAsync(key, model, typeof(T), viewBag: null);
        }

        /// <summary>
        /// Compiles and renders a template with a given <paramref name="key"/>
        /// </summary>
        /// <param name="key">Unique key of the template</param>
        /// <param name="model">Template model</param>
        /// <param name="modelType">Type of the model</param>
        /// <param name="viewBag">Dynamic ViewBag (can be null)</param>
        /// <returns></returns>
        public async Task<string> CompileRenderAsync(string key, object model, Type modelType, ExpandoObject viewBag)
        {
            ITemplatePage template = await GetTemplateAsync(key).ConfigureAwait(false);

            var context = new PageContext(viewBag)
            {
                ExecutingPageKey = key,
                ModelTypeInfo = new ModelTypeInfo(modelType)
            };

            template.PageContext = context;

            return await RenderTemplateAsync(template, model).ConfigureAwait(false);
        }

        /// <summary>
        /// Search and compile a template with a given key
        /// </summary>
        /// <param name="key">Unique key of the template</param>
        /// <param name="compileIfNotCached">If true - it will try to get a template with a specified key and compile it</param>
        /// <returns>An instance of a template</returns>
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

            throw new RazorLightException($"Can't find a template with a specified key ({key})");
        }

        /// <summary>
        /// Renders a template with a given moel
        /// </summary>
        /// <param name="templatePage">Instance of a template</param>
        /// <param name="model">Template model</param>
        /// <returns>Rendered string</returns>
        public async Task<string> RenderTemplateAsync(ITemplatePage templatePage, object model)
        {
            using (var writer = new StringWriter())
            {
                await RenderTemplateAsync(templatePage, model, writer);

                return writer.ToString();
            }
        }

        /// <summary>
        /// Renders a template to the specified <paramref name="textWriter"/>
        /// </summary>
        /// <param name="templatePage">Instance of a template</param>
        /// <param name="model">Template model</param>
        /// <param name="textWriter">Output</param>
        public async Task RenderTemplateAsync(ITemplatePage templatePage, object model, TextWriter textWriter)
        {
            object pageModel = templatePage.PageContext.ModelTypeInfo.CreateTemplateModel(model);
            templatePage.SetModel(pageModel);
            templatePage.Key = templatePage.PageContext.ExecutingPageKey;
            templatePage.PageContext.Writer = textWriter;

            using (var renderer = new TemplateRenderer(templatePage, this, HtmlEncoder.Default))
            {
                await renderer.RenderAsync().ConfigureAwait(false);
            }
        }
    }
}
