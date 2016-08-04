using Microsoft.AspNetCore.Razor;
using RazorLight.Templating;

namespace RazorLight.Compilation
{
    public interface IRazorTemplateCompiler
    {
	    string CompileTemplate(RazorEngineHost host, ITemplateSource templateSource);
    }
}
