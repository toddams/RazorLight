using System;
using System.Dynamic;
using System.Collections.Generic;
using System.IO;
using RazorLight.Rendering;
using RazorLight.Templating;
using System.Linq;

namespace RazorLight
{
	public class RazorLightEngine : IRazorLightEngine
	{
		private readonly IEngineCore core;
		private readonly IPageLookup pageLookup;

		public RazorLightEngine(IEngineCore core, IPageLookup pagelookup)
		{
			if (core == null)
			{
				throw new ArgumentNullException(nameof(core));
			}

			if (pagelookup == null)
			{
				throw new ArgumentNullException();
			}

			this.core = core;
			this.pageLookup = pagelookup;
			this.Configuration = core.Configuration;
		}

		public IEngineConfiguration Configuration { get; }

		public IEngineCore Core => core;

		/// <summary>
		/// Parses a template with a given <paramref name="key" />
		/// </summary>
		/// <typeparam name="T">Type of the Model</typeparam>
		/// <param name="key">Key used to resolve a template</param>
		/// <param name="model">Template model</param>
		/// <returns>Returns parsed string</returns>
		public string Parse<T>(string key, T model)
		{
			return Parse(key, model, typeof(T), viewBag: null);
		}

		/// <summary>
		/// Parses a template with a given <paramref name="key" /> and viewBag
		/// </summary>
		/// <param name="key">Key used to resolve a template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic ViewBag (can be null)</param>
		/// <returns>Returns parsed string</returns>
		/// <remarks>Result is stored in cache</remarks>
		public string Parse<T>(string key, T model, ExpandoObject viewBag)
		{
			return Parse(key, model, typeof(T), viewBag);
		}

		/// <summary>
		/// Parses a template with a given <paramref name="key" /> and viewBag
		/// </summary>
		/// <param name="key">Key used to resolve a template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic ViewBag (can be null)</param>
		/// <param name="prerenderCallbacks">Page specific callback that will be fired before rendering</param>
		/// <returns>Returns parsed string</returns>
		/// <remarks>Result is stored in cache</remarks>
		public string Parse<T>(string key, T model, ExpandoObject viewBag, Action<TemplatePage> prerenderCallback)
		{
			return Parse(key, model, typeof(T), viewBag, prerenderCallback);
		}

		/// <summary>
		/// Parses a template with a given <paramref name="key" />
		/// </summary>
		/// <param name="key">Key used to resolve a template</param>
		/// <param name="model">Template model</param>
		/// <param name="modelType">Type of the model</param>
		/// <param name="viewBag">Dynamic ViewBag (can be null)</param>
		/// <returns>Returns parsed string</returns>
		/// <remarks>Result is stored in cache</remarks>
		public string Parse(string key, object model, Type modelType, ExpandoObject viewBag)
		{
			return Parse(key, model, modelType, viewBag, null);
		}

		/// <summary>
		/// Parses a template with a given <paramref name="key" />
		/// </summary>
		/// <param name="key">Key used to resolve a template</param>
		/// <param name="model">Template model</param>
		/// <param name="modelType">Type of the model</param>
		/// <param name="viewBag">Dynamic ViewBag (can be null)</param>
		/// <param name="prerenderCallbacks">Page specific callback that will be fired before rendering</param>
		/// <returns>Returns parsed string</returns>
		/// <remarks>Result is stored in cache</remarks>
		public string Parse(string key, object model, Type modelType, ExpandoObject viewBag, Action<TemplatePage> prerenderCallback)
		{
			PageLookupResult result = pageLookup.GetPage(key);

			if (!result.Success)
			{
				throw new RazorLightException($"Can't find a view with a specified key ({key})");
			}

			var pageContext = new PageContext(viewBag) { ModelTypeInfo = new ModelTypeInfo(modelType) };
			foreach (var viewStartPage in result.ViewStartEntries)
			{
				pageContext.ViewStartPages.Add(viewStartPage.PageFactory());
			}

			if (prerenderCallback != null)
			{
				pageContext.PrerenderCallbacks.Add(prerenderCallback);
			}

			TemplatePage page = result.ViewEntry.PageFactory();
			page.PageContext = pageContext;

			return RunTemplate(page, model);
		}

		/// <summary>
		/// Creates an instance of the compiled type and casts it to TemplatePage
		/// </summary>
		/// <param name="compiledType">Type to activate</param>
		/// <returns>Template page</returns>
		public TemplatePage Activate(Type compiledType)
		{
			return (TemplatePage)Configuration.Activator.CreateInstance(compiledType);
		}

		/// <summary>
		/// Runs a template, renders a Layout pages and sections.
		/// </summary>
		/// <param name="page">Page to run</param>
		/// <param name="model">Mode of the page</param>
		public string RunTemplate(TemplatePage page, object model)
		{
			object pageModel = page.PageContext.ModelTypeInfo.CreateTemplateModel(model);
			page.SetModel(pageModel);
			page.Path = page.PageContext.ExecutingFilePath;

			using (var writer = new StringWriter())
			{
				page.PageContext.Writer = writer;

				using (var renderer = new PageRenderer(page, pageLookup))
				{
					renderer.ViewStartPages.AddRange(page.PageContext.ViewStartPages);
					renderer.PreRenderCallbacks.AddRange(Configuration.PreRenderCallbacks);
					renderer.RenderAsync(page.PageContext).GetAwaiter().GetResult();
					return writer.ToString();
				}
			}
		}
	}
}
