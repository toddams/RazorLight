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

		/// <inheritdoc cref="IRazorLightEngine"/>
		public Task<string> CompileRenderAsync<T>(string key, T model, ExpandoObject viewBag = null)
		{
			return _handler.CompileRenderAsync(key, model, viewBag);
		}

		/// <inheritdoc cref="IRazorLightEngine"/>
		public Task<string> CompileRenderStringAsync<T>(
			string key,
			string content,
			T model,
			ExpandoObject viewBag = null)
		{
			return _handler.CompileRenderStringAsync(key, content, model, viewBag);
		}

		/// <inheritdoc cref="IRazorLightEngine"/>
		public Task<ITemplatePage> CompileTemplateAsync(string key)
		{
			return _handler.CompileTemplateAsync(key);
		}

		/// <inheritdoc cref="IRazorLightEngine"/>
		public Task<string> RenderTemplateAsync<T>(ITemplatePage templatePage, T model, ExpandoObject viewBag = null)
		{
			return _handler.RenderTemplateAsync(templatePage, model, viewBag);
		}

		/// <inheritdoc cref="IRazorLightEngine"/>
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
