using RazorLight.Compilation;

namespace RazorLight
{
    public interface ILookupCompileProvider
    {
	    CompilationResult CompileLookup(string key);
    }
}
