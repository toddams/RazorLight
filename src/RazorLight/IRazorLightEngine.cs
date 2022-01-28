using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace RazorLight
{
	public interface IRazorLightEngine
	{
		/// <summary>
		/// The Options used to configure RazorLightEngine.
		/// </summary>
		/// <remarks>
		/// Do not call this from your code.  If you need to, use the <see cref="Handler"/> property instead.
		/// </remarks>
		RazorLightOptions Options { get; }

		IEngineHandler Handler { get; }

		/// <summary>
		/// Compiles and renders a template with a given <paramref name="key"/>
		/// </summary>
		/// <typeparam name="T">Type of the model</typeparam>
		/// <param name="key">Unique key of the template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic viewBag of the template</param>
		/// <returns>Rendered template as a string result</returns>
		Task<string> CompileRenderAsync<T>(string key, T model, ExpandoObject viewBag = null);

		[Obsolete("Please, use generic version of CompileRenderAsync", true)]
		Task<string> CompileRenderAsync(string key, object model, Type modelType, ExpandoObject viewBag = null);

		/// <summary>
		/// Compiles and renders a template. Template content is taken directly from <paramref name="content"/> parameter
		/// </summary>
		/// <typeparam name="T">Type of the model</typeparam>
		/// <param name="key">Unique key of the template</param>
		/// <param name="content">Content of the template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic ViewBag</param>
		Task<string> CompileRenderStringAsync<T>(string key, string content, T model, ExpandoObject viewBag = null);

		/// <summary>
		/// Search and compile a template with a given key
		/// </summary>
		/// <param name="key">Unique key of the template</param>
		/// <returns>An instance of a template</returns>
		Task<ITemplatePage> CompileTemplateAsync(string key);

		[Obsolete("Please, use generic version of RenderTemplateAsync", true)]
		Task<string> RenderTemplateAsync(ITemplatePage templatePage, object model, Type modelType, ExpandoObject viewBag = null);

		/// <summary>
		/// Renders a template with a given model
		/// </summary>
		/// <param name="templatePage">Instance of a template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic viewBag of the template</param>
		/// <returns>Rendered string</returns>
		Task<string> RenderTemplateAsync<T>(ITemplatePage templatePage, T model, ExpandoObject viewBag = null);

		[Obsolete("Please, use generic version of RenderTemplateAsync", true)]
		Task RenderTemplateAsync(ITemplatePage templatePage, object model, Type modelType, TextWriter textWriter, ExpandoObject viewBag = null);

		/// <summary>
		/// Renders a template to the specified <paramref name="textWriter"/>
		/// </summary>
		/// <param name="templatePage">Instance of a template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic viewBag of the page</param>
		/// <param name="textWriter">Output</param>
		Task RenderTemplateAsync<T>(
			ITemplatePage templatePage,
			T model,
			TextWriter textWriter,
			ExpandoObject viewBag = null);
	}
}