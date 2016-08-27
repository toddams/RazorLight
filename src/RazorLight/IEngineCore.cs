using System;
using RazorLight.Compilation;
using RazorLight.Templating;

namespace RazorLight
{
	public interface IEngineCore
	{
		IEngineConfiguration Configuration { get; }

		/// <summary>
		/// Generates razor template by parsing given <param name="templateSource" />
		/// </summary>
		/// <param name="templateSource"></param>
		/// <param name="modelTypeInfo"></param>
		/// <returns></returns>
		string GenerateRazorTemplate(ITemplateSource templateSource, ModelTypeInfo modelTypeInfo);

		/// <summary>
		/// Compiles a <see cref="ITemplateSource"/> with a specified <see cref="ModelTypeInfo"/>
		/// </summary>
		/// <param name="templateSource">Template source</param>
		/// <param name="modelTypeInfo">Model type information</param>
		/// <returns>Compiled type in succeded. Compilation errors on fail</returns>
		CompilationResult CompileSource(ITemplateSource templateSource, ModelTypeInfo modelTypeInfo);

		/// <summary>
		/// Compiles a page with a specified <param name="key" />
		/// </summary>
		/// <param name="key"></param>
		/// <returns>Compiled type in succeded. Compilation errors on fail</returns>
		CompilationResult KeyCompile(string key);

		/// <summary>
		/// Activates a type using Activator from <see cref="IEngineConfiguration"/>
		/// </summary>
		/// <param name="compiledType"></param>
		/// <returns></returns>
		TemplatePage Activate(Type compiledType);

	}
}
