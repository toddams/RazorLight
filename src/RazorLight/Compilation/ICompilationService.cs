using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RazorLight.Generation;
using System.Reflection;

namespace RazorLight.Compilation
{
	public interface ICompilationService
	{
		CSharpCompilationOptions CSharpCompilationOptions { get; }
		EmitOptions EmitOptions { get; }
		CSharpParseOptions ParseOptions { get; }
		Assembly OperatingAssembly { get; }

		Assembly CompileAndEmit(IGeneratedRazorTemplate razorTemplate);
	}
}