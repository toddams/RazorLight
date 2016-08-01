using System;
using RazorLight.Compilation;

namespace RazorLight.Abstractions
{
	public interface ICompilerCache
    {
		CompilerCacheResult GetOrAdd(string relativePath, Func<string, CompilationResult> compile);
    }
}
