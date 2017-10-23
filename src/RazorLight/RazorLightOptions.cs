using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RazorLight
{
    public class RazorLightOptions
    {
        public RazorLightOptions()
        {
            Namespaces = new List<string>();
            DynamicTemplates = new ConcurrentDictionary<string, string>();
            AdditionalMetadataReferences = new HashSet<MetadataReference>();
        }

        public ICollection<string> Namespaces { get; set; }

        public IDictionary<string, string> DynamicTemplates { get; set; }

        public HashSet<MetadataReference> AdditionalMetadataReferences { get; set; }
    }
}
