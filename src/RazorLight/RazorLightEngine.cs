using RazorLight.Caching;
using System;
using System.Dynamic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace RazorLight
{
    public class RazorLightEngine : IRazorLightEngine
    {
        private ITemplateFactoryProvider templateFactoryProvider;
        private ICachingProvider cache;

        public RazorLightEngine(
            RazorLightOptions options,
            ITemplateFactoryProvider factoryProvider,
            ICachingProvider cachingProvider)
        {
            Options = options;
            templateFactoryProvider = factoryProvider;
            cache = cachingProvider;
        }

        public ICachingProvider TemplateCache => cache;

        public RazorLightOptions Options { get; }

        /// <summary>
        /// Compiles and renders a template with a given <paramref name="key"/>
        /// </summary>
        /// <typeparam name="T">Type of the model</typeparam>
        /// <param name="key">Unique key of the template</param>
        /// <param name="model">Template model</param>
        /// <param name="viewBag">Dynamic viewBag of the template</param>
        /// <returns>Rendered template as a string result</returns>
        public Task<string> CompileRenderAsync<T>(string key, T model, ExpandoObject viewBag = null)
        {
            return CompileRenderAsync(key, model, typeof(T), viewBag);
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
            ITemplatePage template = await CompileTemplateAsync(key).ConfigureAwait(false);

            return await RenderTemplateAsync(template, model, modelType, viewBag).ConfigureAwait(false);
        }

        /// <summary>
        /// Search and compile a template with a given key
        /// </summary>
        /// <param name="key">Unique key of the template</param>
        /// <param name="compileIfNotCached">If true - it will try to get a template with a specified key and compile it</param>
        /// <returns>An instance of a template</returns>
        public async Task<ITemplatePage> CompileTemplateAsync(string key)
        {
            var cacheLookupResult = cache.RetrieveTemplate(key);
            if (cacheLookupResult.Success)
            {
                return cacheLookupResult.Template.TemplatePageFactory();
            }

            var pageFactoryResult = await templateFactoryProvider.CreateFactoryAsync(key).ConfigureAwait(false);
            cache.CacheTemplate(key, pageFactoryResult.TemplatePageFactory);

            return pageFactoryResult.TemplatePageFactory();
        }

        /// <summary>
        /// Renders a template with a given model
        /// </summary>
        /// <param name="templatePage">Instance of a template</param>
        /// <param name="model">Template model</param>
        /// <param name="modelType">Type of the model</param>
        /// <param name="viewBag">Dynamic viewBag of the template</param>
        /// <returns>Rendered string</returns>
        public async Task<string> RenderTemplateAsync(ITemplatePage templatePage, object model, Type modelType, ExpandoObject viewBag = null)
        {
            using (var writer = new StringWriter())
            {
                await RenderTemplateAsync(templatePage, model, modelType, writer);
                string result = writer.ToString();

                return result;
            }
        }

        /// <summary>
        /// Renders a template to the specified <paramref name="textWriter"/>
        /// </summary>
        /// <param name="templatePage">Instance of a template</param>
        /// <param name="model">Template model</param>
        /// <param name="modelType">Type of the model</param>
        /// <param name="viewBag">Dynamic viewBag of the page</param>
        /// <param name="textWriter">Output</param>
        public async Task RenderTemplateAsync(
            ITemplatePage templatePage,
            object model, Type modelType,
            TextWriter textWriter,
            ExpandoObject viewBag = null)
        {
            var pageContext = new PageContext(viewBag)
            {
                ExecutingPageKey = templatePage.Key,
                Writer = textWriter
            };

            if (model != null)
            {
                pageContext.ModelTypeInfo = new ModelTypeInfo(modelType);

                object pageModel = pageContext.ModelTypeInfo.CreateTemplateModel(model);
                templatePage.SetModel(pageModel);
            }

            templatePage.PageContext = pageContext;

            using (var renderer = new TemplateRenderer(templatePage, this, HtmlEncoder.Default))
            {
                await renderer.RenderAsync().ConfigureAwait(false);
            }
        }
    }
}
