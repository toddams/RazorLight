using RazorLight.Compilation;
using RazorLight.Templating;
using System;

namespace RazorLight.Extensions
{
    public static class ParsingExtensions
    {
		/// <summary>
		/// Parses a string
		/// </summary>
		/// <typeparam name="T">Type of the model</typeparam>
		/// <param name="content">Template to parse</param>
		/// <param name="model">Template model</param>
		/// <returns>Returns parsed string</returns>
		/// <remarks>Result is not cached</remarks>
		public static string ParseString<T>(this IRazorLightEngine engine, string content, T model)
		{
			return engine.ParseString(content, model, typeof(T));
		}

		/// <summary>
		/// Parses a string
		/// </summary>
		/// <param name="content">Template to parse</param>
		/// <param name="model">Template model</param>
		/// <param name="modelType">Type of the model</param>
		/// <returns></returns>
		public static string ParseString(this IRazorLightEngine engine, string content, object model, Type modelType)
		{
			if (string.IsNullOrEmpty(content))
			{
				throw new ArgumentNullException(nameof(content));
			}

			ITemplateSource templateSource = new LoadedTemplateSource(content);

			ModelTypeInfo modelTypeInfo = new ModelTypeInfo(modelType);
			CompilationResult result = engine.Core.CompileSource(templateSource, modelTypeInfo);
			result.EnsureSuccessful();

			TemplatePage page = engine.Activate(result.CompiledType);
			page.PageContext = new PageContext() { ModelTypeInfo = modelTypeInfo };

			return engine.RunTemplate(page, model);
		}
	}
}
