using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace RazorLight
{
	public class RazorLightEngine : IRazorLightEngine
    {
		private readonly IEngineHandler _handler;

		public RazorLightEngine(IEngineHandler handler)
        {
			_handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

		public RazorLightOptions Options => Handler.Options;

		public IEngineHandler Handler => _handler;

		[Obsolete("Please, use generic version of CompileRenderAsync", true)]
		public Task<string> CompileRenderAsync(string key, object model, Type modelType, ExpandoObject viewBag = null)
		{
			throw new NotImplementedException();
		}

		[Obsolete("Please, use CompileRenderStringAsync", true)]
		public Task<string> CompileRenderAsync(
			string key,
			string content,
			object model,
			Type modelType,
			ExpandoObject viewBag = null)
		{
			throw new NotImplementedException();
		}

		[Obsolete("Please, use generic version of RenderTemplateAsync", true)]
		public Task<string> RenderTemplateAsync(ITemplatePage templatePage, object model, Type modelType, ExpandoObject viewBag = null)
		{
			throw new NotImplementedException();
		}

		[Obsolete("Please, use generic version of RenderTemplateAsync", true)]
		public Task RenderTemplateAsync(ITemplatePage templatePage, object model, Type modelType, TextWriter textWriter, ExpandoObject viewBag = null)
		{
			throw new NotImplementedException();
		}

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
			return _handler.CompileRenderAsync(key, model, viewBag);
        }

		/// <summary>
		/// Compiles and renders a template. Template content is taken directly from <paramref name="content"/> parameter
		/// </summary>
		/// <typeparam name="T">Type of the model</typeparam>
		/// <param name="key">Unique key of the template</param>
		/// <param name="content">Content of the template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic ViewBag</param>
		public Task<string> CompileRenderStringAsync<T>(
			string key,
			string content,
			T model,
			ExpandoObject viewBag = null)
		{
			return _handler.CompileRenderStringAsync(key, content, model, viewBag);
		}

		/// <summary>
		/// Search and compile a template with a given key
		/// </summary>
		/// <param name="key">Unique key of the template</param>
		/// <param name="compileIfNotCached">If true - it will try to get a template with a specified key and compile it</param>
		/// <returns>An instance of a template</returns>
		public Task<ITemplatePage> CompileTemplateAsync(string key)
        {
			return _handler.CompileTemplateAsync(key);
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
            return _handler.RenderTemplateAsync(templatePage, model, viewBag);
        }

        /// <summary>
        /// Renders a template to the specified <paramref name="textWriter"/>
        /// </summary>
        /// <param name="templatePage">Instance of a template</param>
        /// <param name="model">Template model</param>
        /// <param name="modelType">Type of the model</param>
        /// <param name="viewBag">Dynamic viewBag of the page</param>
        /// <param name="textWriter">Output</param>
        public Task RenderTemplateAsync<T>(
            ITemplatePage templatePage,
            T model, 
            TextWriter textWriter,
            ExpandoObject viewBag = null)
        {
			return _handler.RenderTemplateAsync(templatePage, model, textWriter, viewBag);
        }
	}
}
