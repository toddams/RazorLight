using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace RazorLight.Compilation
{
	public interface IRazorTemplateCompiler
	{
		ICompilationService CompilationService { get; }
		
		IMemoryCache Cache { get; }

		Task<CompiledTemplateDescriptor> CompileAsync(string templateKey);
	}
}