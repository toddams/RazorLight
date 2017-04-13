using RazorLight.Internal;
using System;
using System.Dynamic;

namespace RazorLight
{
	public interface IRazorLightEngine
    {
		IEngineConfiguration Configuration { get; }

		IEngineCore Core { get; }

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
		/// Creates an instance of the compiled type and casts it to TemplatePage
		/// </summary>
		/// <param name="compiledType">Type to activate</param>
		/// <returns>Template page</returns>
		ITemplatePage Activate(Type compiledType);

		/// <summary>
		/// Runs a template, renders a Layout pages and sections.
		/// </summary>
		/// <param name="page">Page to run</param>
		/// <param name="model">Mode of the page</param>
		string RunTemplate(ITemplatePage page, object model);
	}
}
