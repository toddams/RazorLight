using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace RazorLight
{
    public static class RazorLightExtensions
    {
        public static Task<string> CompileRenderAsync<T>(
            this IRazorLightEngine engine,
            string key,
            string content,
            T model,
            ExpandoObject viewBag = null)
        {
            return CompileRenderAsync(engine, key, content, model, typeof(T), viewBag);
        }

        public static Task<string> CompileRenderAsync(
            this IRazorLightEngine engine, 
            string key, 
            string content, 
            object model,
            Type modelType,
            ExpandoObject viewBag = null)
        {
            engine.Options.DynamicTemplates[key] = content;
            return engine.CompileRenderAsync(key, model, modelType, viewBag);
        }

    }
}
