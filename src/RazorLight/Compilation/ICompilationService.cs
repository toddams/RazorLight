using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RazorLight.Generation;

namespace RazorLight.Compilation
{
    public interface ICompilationService
    {
        CSharpCompilationOptions CSharpCompilationOptions { get; }
        EmitOptions EmitOptions { get; }
        CSharpParseOptions ParseOptions { get; }

        CompiledTemplateDescriptor CompileAndEmit(IGeneratedRazorTemplate razorTemplate);
    }
}