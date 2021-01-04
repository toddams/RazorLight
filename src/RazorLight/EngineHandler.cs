using System;
using System.Dynamic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Internal.Buffering;

namespace RazorLight
{
	public class EngineHandler : IEngineHandler
	{
		public EngineHandler(
			RazorLightOptions options,
			IRazorTemplateCompiler compiler,
			ITemplateFactoryProvider factoryProvider,
			ICachingProvider cache)
		{
			Options = options ?? throw new ArgumentNullException(nameof(options));
			Compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
			FactoryProvider = factoryProvider ?? throw new ArgumentNullException(nameof(factoryProvider));

			Cache = cache;
		}

		public EngineHandler(
			IOptions<RazorLightOptions> options,
			IRazorTemplateCompiler compiler,
			ITemplateFactoryProvider factoryProvider,
			ICachingProvider cache) : this(options.Value, compiler, factoryProvider, cache)
		{
			

		}

		public RazorLightOptions Options { get; }
		public ICachingProvider Cache { get; }
		public IRazorTemplateCompiler Compiler { get; }
		public ITemplateFactoryProvider FactoryProvider { get; }

		public bool IsCachingEnabled => Cache != null;

		/// <summary>
		/// Search and compile a template with a given key
		/// </summary>
		/// <param name="key">Unique key of the template</param>
		/// <returns>An instance of a template</returns>
		public async Task<ITemplatePage> CompileTemplateAsync(string key)
		{
			ITemplatePage templatePage = null;
			if (IsCachingEnabled)
			{
				var cacheLookupResult = Cache.RetrieveTemplate(key);
				if (cacheLookupResult.Success)
				{
					templatePage = cacheLookupResult.Template.TemplatePageFactory();
				}
			}

			if(templatePage == null)
			{
				CompiledTemplateDescriptor templateDescriptor = await Compiler.CompileAsync(key);
				Func<ITemplatePage> templateFactory = FactoryProvider.CreateFactory(templateDescriptor);

				if(IsCachingEnabled) {
					Cache.CacheTemplate(
					key,
					templateFactory,
					templateDescriptor.ExpirationToken);
				}

				templatePage = templateFactory();
			}

			templatePage.DisableEncoding = Options.DisableEncoding ?? false;
			return templatePage;
		}

		/// <summary>
		/// Renders a template with a given model
		/// </summary>
		/// <param name="templatePage">Instance of a template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic viewBag of the template</param>
		/// <returns>Rendered string</returns>
		public async Task<string> RenderTemplateAsync<T>(ITemplatePage templatePage, T model, ExpandoObject viewBag = null)
		{
			using (var writer = new StringWriter())
			{
				await RenderTemplateAsync(templatePage, model, writer, viewBag);

				return writer.ToString();
			}
		}

		/// <summary>
		/// Renders a template to the specified <paramref name="textWriter"/>
		/// </summary>
		/// <param name="templatePage">Instance of a template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic viewBag of the page</param>
		/// <param name="textWriter">Output</param>
		public async Task RenderTemplateAsync<T>(
			ITemplatePage templatePage,
			T model,
			TextWriter textWriter,
			ExpandoObject viewBag = null)
		{
			SetModelContext(templatePage, textWriter, model, viewBag);

			using (var scope = new MemoryPoolViewBufferScope())
			{
				var renderer = new TemplateRenderer(this, HtmlEncoder.Default, scope);
				await renderer.RenderAsync(templatePage).ConfigureAwait(false);
			}
		}

		public async Task RenderIncludedTemplateAsync<T>(
			ITemplatePage templatePage,
			T model,
			TextWriter textWriter,
			ExpandoObject viewBag,
			TemplateRenderer templateRenderer)
		{
			SetModelContext(templatePage, textWriter, model, viewBag);
			await templateRenderer.RenderAsync(templatePage).ConfigureAwait(false);
		}

		/// <summary>
		/// Compiles and renders a template with a given <paramref name="key"/>
		/// </summary>
		/// <param name="key">Unique key of the template</param>
		/// <param name="model">Template model</param>
		/// <param name="viewBag">Dynamic ViewBag (can be null)</param>
		/// <returns></returns>
		public async Task<string> CompileRenderAsync<T>(string key, T model, ExpandoObject viewBag = null)
		{
			ITemplatePage template = await CompileTemplateAsync(key).ConfigureAwait(false);

			return await RenderTemplateAsync(template, model, viewBag).ConfigureAwait(false);
		}

		/// <summary>
		/// Compiles and renders a template. Template content is taken directly from <paramref name="content"/> parameter
		/// </summary>
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
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (string.IsNullOrEmpty(content))
			{
				throw new ArgumentNullException(nameof(content));
			}

			Options.DynamicTemplates[key] = content;
			return CompileRenderAsync(key, model, viewBag);
		}

		private void SetModelContext<T>(
			ITemplatePage templatePage,
			TextWriter textWriter,
			T model,
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
				pageContext.ModelTypeInfo = new ModelTypeInfo(model.GetType());

				object pageModel = pageContext.ModelTypeInfo.CreateTemplateModel(model);
				templatePage.SetModel(pageModel);

				pageContext.Model = pageModel;
			}

			templatePage.PageContext = pageContext;
		}
	}
}
