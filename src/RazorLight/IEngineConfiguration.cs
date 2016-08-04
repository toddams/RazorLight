using System.Collections.Generic;
using RazorLight.Compilation;

namespace RazorLight
{
    public interface IEngineConfiguration
    {
		IActivator Activator { get; }

		IRazorTemplateCompiler RazorTemplateCompiler { get; }

		ICompilerService CompilerService { get; }

		ISet<string> Namespaces { get; set; }
	}
}
