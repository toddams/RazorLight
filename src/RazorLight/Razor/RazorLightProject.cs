using System.Collections.Generic;

namespace RazorLight.Razor
{
    public abstract class RazorLightProject
    {
        public abstract RazorLightProjectItem GetItem(string templateKey);

        public abstract IEnumerable<RazorLightProjectItem> GetImports(string templateKey);
    }
}
