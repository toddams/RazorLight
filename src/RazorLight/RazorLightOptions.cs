using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RazorLight
{
    public class RazorLightOptions
    {
        public ICollection<string> Namespaces { get; set; }

        public IDictionary<string, string> DynamicTemplates { get; set; }

        public static RazorLightOptions Default => new RazorLightOptions()
        {
            Namespaces = new List<string>(),
            DynamicTemplates = new ConcurrentDictionary<string, string>()
        };
    }
}
