using System;

namespace RazorLight.Abstractions
{
    interface ICompilerCache
    {
	    string GetOrAdd(string relativePath, Func<string, string> compile);
    }
}
