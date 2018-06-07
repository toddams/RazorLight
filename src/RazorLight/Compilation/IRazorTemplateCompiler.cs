using System.Threading.Tasks;

namespace RazorLight.Compilation
{
	public interface IRazorTemplateCompiler
	{
		ICompilationService CompilationService { get; }

		Task<CompiledTemplateDescriptor> CompileAsync(string templateKey);
	}
}