using Microsoft.Extensions.Primitives;
using RazorLight.Razor;

namespace RazorLight.Compilation
{
    public class CompiledTemplateDescriptor
    {
        public string TemplateKey { get; set; }

        public RazorLightTemplateAttribute TemplateAttribute { get; set; }

        public IChangeToken ExpirationToken { get; set; }

        public bool IsPrecompiled { get; set; }
    }
}
