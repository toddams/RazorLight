using RazorLight.Internal;
using System;
using System.Dynamic;

namespace RazorLight
{
	public interface IRazorLightEngine
    {
		IEngineConfiguration Configuration { get; }

		PreRenderActionList PreRenderCallbacks { get; }

		/// <summary>
		/// Parses a template with a given <paramref name="key" />
		/// </summary>
		/// <typeparam name="T">Type of the Model</typeparam>
		/// <param name="key">Key used to resolve a template</param>
		/// <param name="model">Template model</param>
		/// <returns>Returns parsed string</returns>
		string Parse<T>(string key, T model);

		/// <summary>
		/// Parses a template with a given <paramref name="key" /> and viewBag
		/// </summary>
		/// <param name="key">Key used to resolve a template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic ViewBag (can be null)</param>
		/// <returns>Returns parsed string</returns>
		/// <remarks>Result is stored in cache</remarks>
		string Parse<T>(string key, T model, ExpandoObject viewBag);

		/// <summary>
		/// Parses a template with a given <paramref name="key" />
		/// </summary>
		/// <param name="key">Key used to resolve a template</param>
		/// <param name="model">Template model</param>
		/// <param name="modelType">Type of the model</param>
		/// <param name="viewBag">Dynamic ViewBag (can be null)</param>
		/// <returns>Returns parsed string</returns>
		/// <remarks>Result is stored in cache</remarks>
		string Parse(string key, object model, Type modelType, ExpandoObject viewBag);

		/// <summary>
		/// Parses a string
		/// </summary>
		/// <typeparam name="T">Type of the model</typeparam>
		/// <param name="content">Template to parse</param>
		/// <param name="model">Template model</param>
		/// <returns>Returns parsed string</returns>
		/// <remarks>Result is not cached</remarks>
		string ParseString<T>(string content, T model);

		/// <summary>
		/// Parses a string
		/// </summary>
		/// <param name="content">Template to parse</param>
		/// <param name="model">Template model</param>
		/// <param name="modelType">Type of the model</param>
		/// <returns></returns>
		string ParseString(string content, object model, Type modelType);

		/// <summary>
		/// Creates an instance of the compiled type and casts it to TemplatePage
		/// </summary>
		/// <param name="compiledType">Type to activate</param>
		/// <returns>Template page</returns>
		TemplatePage Activate(Type compiledType);

		/// <summary>
		/// Runs a template, renders a Layout pages and sections.
		/// </summary>
		/// <param name="page">Page to run</param>
		/// <param name="model">Mode of the page</param>
		string RunTemplate(TemplatePage page, object model);
	}
}
