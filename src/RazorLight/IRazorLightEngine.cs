using System;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace RazorLight
{
    public interface IRazorLightEngine
    {
        Task<string> CompileRenderAsync(string key, object model, Type modelType, ExpandoObject viewBag);
        Task<string> CompileRenderAsync<T>(string key, T model);
        Task<ITemplatePage> GetTemplateAsync(string key, bool compileIfNotCached = true);
        Task<string> RenderTemplateAsync(ITemplatePage templatePage, object model);
        Task RenderTemplateAsync(ITemplatePage templatePage, object model, TextWriter textWriter);
    }
}