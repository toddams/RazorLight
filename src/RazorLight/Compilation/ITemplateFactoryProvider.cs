using System.Threading.Tasks;
using RazorLight.Generation;
using RazorLight.Razor;

namespace RazorLight.Compilation
{
    public interface ITemplateFactoryProvider
    {
        Task<TemplateFactoryResult> CreateFactoryAsync(string templateKey);
        Task<TemplateFactoryResult> CreateFactoryAsync(RazorLightProjectItem projectItem);

		RazorSourceGenerator SourceGenerator { get; }
		RoslynCompilationService Compiler { get; }
	}
}