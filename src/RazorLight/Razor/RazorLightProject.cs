using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorLight.Razor
{
    public abstract class RazorLightProject
    {
        public abstract Task<RazorLightProjectItem> GetItemAsync(string templateKey);

        public abstract Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey);
    }
}
