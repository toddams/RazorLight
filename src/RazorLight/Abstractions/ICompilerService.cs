using System;
using RazorLight.Compilation;

namespace RazorLight.Abstractions
{
    public interface ICompilerService
    {
		CompilationResult Compile(string content);
    }
}
