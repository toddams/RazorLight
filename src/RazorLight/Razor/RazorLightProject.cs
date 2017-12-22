using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorLight.Razor
{
    public abstract class RazorLightProject
    {
        /// <summary>
        /// Looks up for the template source with a given <paramref name="templateKey"/>
        /// </summary>
        /// <param name="templateKey">Unique template key</param>
        /// <returns></returns>
        public abstract Task<RazorLightProjectItem> GetItemAsync(string templateKey);

        /// <summary>
        /// Looks up for the ViewImports content for the given template
        /// </summary>
        /// <param name="templateKey"></param>
        /// <returns></returns>
        public abstract Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey);
    }
}
