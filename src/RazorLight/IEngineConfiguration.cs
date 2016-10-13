using System.Collections.Generic;
using RazorLight.Compilation;
using RazorLight.Internal;

namespace RazorLight
{
	public interface IEngineConfiguration
	{
		/// <summary>
		/// Activator used to create an instance of the compiled templates
		/// </summary>
		IActivator Activator { get; }

		/// <summary>
		/// Class used to compile razor templates into *.cs file
		/// </summary>
		IRazorTemplateCompiler RazorTemplateCompiler { get; }

		/// <summary>
		/// Class used to compile razor templates
		/// </summary>
		ICompilerService CompilerService { get; }

		/// <summary>
		/// Additional namespace to include into template (_ViewImports like)
		/// </summary>
		ISet<string> Namespaces { get; }

		PreRenderActionList PreRenderCallbacks { get; }
	}
}
