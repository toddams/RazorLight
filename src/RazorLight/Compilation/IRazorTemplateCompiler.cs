using RazorLight.Host;
using RazorLight.Templating;

namespace RazorLight.Compilation
{
	public interface IRazorTemplateCompiler
	{
		string CompileTemplate(RazorLightHost host, ITemplateSource templateSource);
	}
}
