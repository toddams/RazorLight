using System.Threading.Tasks;
using RazorLight.Razor;

namespace RazorLight
{
    public interface ITemplateFactoryProvider
    {
        Task<TemplateFactoryResult> CreateFactoryAsync(string templateKey);
        Task<TemplateFactoryResult> CreateFactoryAsync(RazorLightProjectItem projectItem);
    }
}