using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;

namespace RazorLight
{
	public class RazorLightOptions
	{
		public RazorLightOptions()
		{
			Namespaces = new List<string>();
			DynamicTemplates = new ConcurrentDictionary<string, string>();
			AdditionalMetadataReferences = new HashSet<MetadataReference>();
			PreRenderCallbacks = new List<Action<ITemplatePage>>();
		}

		public ICollection<string> Namespaces { get; set; }

		public IDictionary<string, string> DynamicTemplates { get; set; }

		public HashSet<MetadataReference> AdditionalMetadataReferences { get; set; }

		public virtual IList<Action<ITemplatePage>> PreRenderCallbacks { get; set; }
	}
}
