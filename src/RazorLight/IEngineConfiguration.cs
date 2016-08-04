using RazorLight.Compilation;
using RazorLight.Internal;

namespace RazorLight
{
    public interface IEngineConfiguration
    {
		IActivator Activator { get; }

		IRazorTemplateCompiler RazorTemplateCompiler { get; }

		ICompilerService CompilerService { get; }
    }
}
