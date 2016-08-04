using System;
using RazorLight.Compilation;

namespace RazorLight.Caching
{
	public interface ICompilerCache
	{
		CompilerCacheResult GetOrAdd(string relativePath, Func<string, CompilationResult> compile);
	}
}
