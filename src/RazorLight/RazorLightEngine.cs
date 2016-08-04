using System;
using System.IO;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Host;
using RazorLight.Rendering;
using RazorLight.Templating;

namespace RazorLight
{
	public class RazorLightEngine
	{
		private readonly IEngineCore core;
		private readonly IPageLookup pageLookup;

		public RazorLightEngine(IEngineCore core, IPageLookup pagelookup)
		{
			this.core = core;
			this.pageLookup = pagelookup;
			this.Configuration = core.Configuration;
		}

		public IEngineConfiguration Configuration { get; }
		
		public string Parse<T>(string key, T model)
		{
			return Parse(key, model, typeof(T));
		}

		public string Parse(string key, object model, Type modelType)
		{
			PageCacheResult result = pageLookup.GetPage(key);

			if (!result.Success)
			{
				throw new RazorLightException($"Can't find a view with a specified key ({key})");
			}

			TemplatePage page = result.ViewEntry.PageFactory();
			page.PageContext = new PageContext()
			{
				ModelTypeInfo = new ModelTypeInfo(modelType)
			};

			return RunTemplate(page, model);
		}

		public string ParseString<T>(string content, T model)
		{
			return ParseString(content, model, typeof(T));
		}

		public string ParseString(string content, object model, Type modelType)
		{
			if (string.IsNullOrEmpty(content))
			{
				throw new ArgumentNullException(nameof(content));
			}

			ITemplateSource templateSource = new StringTemplateSource(content);

			ModelTypeInfo modelTypeInfo = new ModelTypeInfo(modelType);
			CompilationResult result = core.CompileSource(templateSource, modelTypeInfo);
			result.EnsureSuccessful();

			TemplatePage page = Activate(result.CompiledType);
			page.PageContext = new PageContext() { ModelTypeInfo = modelTypeInfo };

			return RunTemplate(page, model);
		}

		public TemplatePage Activate(Type compiledType)
		{
			return (TemplatePage) Configuration.Activator.CreateInstance(compiledType);
		}

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
					renderer.RenderAsync(page.PageContext).Wait();
					return writer.ToString();
				}
			}
		}
	}
}
