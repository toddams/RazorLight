using RazorLight.Caching;
using System;
using System.Dynamic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using RazorLight.Compilation;
using RazorLight.Internal;

namespace RazorLight
{
    public class RazorLightEngine : IRazorLightEngine
    {
		public RazorLightEngine(
			RazorLightOptions options,
			IRazorTemplateCompiler templateCompiler,
			ITemplateFactoryProvider factoryProvider,
            ICachingProvider cachingProvider)
        {
			Options = options ?? throw new ArgumentNullException(nameof(options));
			TemplateFactoryProvider = factoryProvider ?? throw new ArgumentNullException(nameof(factoryProvider));
			TemplateCompiler = templateCompiler ?? throw new ArgumentNullException(nameof(templateCompiler));

			TemplateCache = cachingProvider;
        }

		public RazorLightOptions Options { get; }
		public ICachingProvider TemplateCache { get; }
		public IRazorTemplateCompiler TemplateCompiler { get; }
		public ITemplateFactoryProvider TemplateFactoryProvider { get; }

		public bool IsCachingEnabled => TemplateCache != null;

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
        public async Task<string> CompileRenderAsync(string key, object model, Type modelType, ExpandoObject viewBag = null)
        {
            ITemplatePage template = await CompileTemplateAsync(key).ConfigureAwait(false);

            return await RenderTemplateAsync(template, model, modelType, viewBag).ConfigureAwait(false);
        }

		/// <summary>
		/// Compiles and renders a template. Template content is taken directly from <paramref name="content"/> parameter
		/// </summary>
		/// <typeparam name="T">Type of the model</typeparam>
		/// <param name="key">Unique key of the template</param>
		/// <param name="content">Content of the template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic ViewBag</param>
		public Task<string> CompileRenderAsync<T>(
			string key,
			string content,
			T model,
			ExpandoObject viewBag = null)
		{
			return CompileRenderAsync(key, content, model, typeof(T), viewBag);
		}

		/// <summary>
		/// Compiles and renders a template. Template content is taken directly from <paramref name="content"/> parameter
		/// </summary>
		/// <param name="key">Unique key of the template</param>
		/// <param name="content">Content of the template</param>
		/// <param name="model">Template model</param>
		/// <param name="modelType">Type of the model</param>
		/// <param name="viewBag">Dynamic ViewBag</param>
		public Task<string> CompileRenderAsync(
			string key,
			string content,
			object model,
			Type modelType,
			ExpandoObject viewBag = null)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (string.IsNullOrEmpty(content))
			{
				throw new ArgumentNullException(nameof(content));
			}

			Options.DynamicTemplates[key] = content;
			return CompileRenderAsync(key, model, modelType, viewBag);
		}

		/// <summary>
		/// Search and compile a template with a given key
		/// </summary>
		/// <param name="key">Unique key of the template</param>
		/// <param name="compileIfNotCached">If true - it will try to get a template with a specified key and compile it</param>
		/// <returns>An instance of a template</returns>
		public async Task<ITemplatePage> CompileTemplateAsync(string key)
        {
			if (IsCachingEnabled)
			{
				var cacheLookupResult = TemplateCache.RetrieveTemplate(key);
				if(cacheLookupResult.Success)
				{
					return cacheLookupResult.Template.TemplatePageFactory();
				}
			}

			CompiledTemplateDescriptor templateDescriptor = await TemplateCompiler.CompileAsync(key);
			Func<ITemplatePage> templateFactory = TemplateFactoryProvider.CreateFactory(templateDescriptor);

			if (IsCachingEnabled)
			{
				TemplateCache.CacheTemplate(
				key,
				templateFactory,
				templateDescriptor.ExpirationToken);
			}

			return templateFactory();
        }

        /// <summary>
        /// Renders a template with a given model
        /// </summary>
        /// <param name="templatePage">Instance of a template</param>
        /// <param name="model">Template model</param>
        /// <param name="viewBag">Dynamic viewBag of the template</param>
        /// <returns>Rendered string</returns>
        public Task<string> RenderTemplateAsync<T>(ITemplatePage templatePage, T model, ExpandoObject viewBag = null)
        {
            return RenderTemplateAsync(templatePage, model, typeof(T), viewBag);
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
                await RenderTemplateAsync(templatePage, model, modelType, writer, viewBag);

                return writer.ToString();
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
            object model, 
			Type modelType,
            TextWriter textWriter,
            ExpandoObject viewBag = null)
        {
			SetModelContext(templatePage, textWriter, model, modelType, viewBag);

			using (var scope = new MemoryPoolViewBufferScope())
			{
				var renderer = new TemplateRenderer(templatePage, this, HtmlEncoder.Default, scope);
				await renderer.RenderAsync().ConfigureAwait(false);
			}
        }

		public async Task RenderIncludedTemplateAsync(
			ITemplatePage templatePage,
			object model,
			Type modelType,
			TextWriter textWriter,
			ExpandoObject viewBag,
			TemplateRenderer templateRenderer)
		{
			SetModelContext(templatePage, textWriter, model, modelType, viewBag);

			templateRenderer.RazorPage = templatePage;
			await templateRenderer.RenderAsync().ConfigureAwait(false);
		}

		private void SetModelContext(
			ITemplatePage templatePage,
			TextWriter textWriter,
			object model,
			Type modelType,
			ExpandoObject viewBag)
		{
			if (textWriter == null)
			{
				throw new ArgumentNullException(nameof(textWriter));
			}

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
		}
	}
}
