using System.Threading.Tasks;

namespace RazorLight
{
    public interface ITemplateFactoryProvider
    {
        Task<TemplateFactoryResult> CreateFactoryAsync(string templateKey);
    }
}