using System.Threading.Tasks;
using RazorLight.Compilation;
using RazorLight.Razor;

namespace RazorLight
{
    public interface ITemplateFactoryProvider
    {
        Task<TemplateFactoryResult> CreateFactoryAsync(string templateKey);
        Task<TemplateFactoryResult> CreateFactoryAsync(RazorLightProjectItem projectItem);

		RazorSourceGenerator SourceGenerator { get; }
		RoslynCompilationService Compiler { get; }
	}
}