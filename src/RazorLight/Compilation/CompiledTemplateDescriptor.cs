using Microsoft.Extensions.Primitives;
using RazorLight.Razor;
using System.Collections.Generic;

namespace RazorLight.Compilation
{
    public class CompiledTemplateDescriptor
    {
        public string TemplateKey { get; set; }

        public RazorLightTemplateAttribute TemplateAttribute { get; set; }

        public IList<IChangeToken> ExpirationTokens { get; set; }

        public bool IsPrecompiled { get; set; }
    }
}
