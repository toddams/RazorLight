using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace RazorLight.Compilation
{
    public interface ICompilationService
    {
        CSharpCompilationOptions CSharpCompilationOptions { get; }
        EmitOptions EmitOptions { get; }
        CSharpParseOptions ParseOptions { get; }

        Assembly CompileAndEmit(string generatedCode);
    }
}