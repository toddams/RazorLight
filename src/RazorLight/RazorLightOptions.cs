using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using RazorLight.Caching;

namespace RazorLight
{
	public class RazorLightOptions
	{
		public RazorLightOptions()
		{
			Namespaces = new HashSet<string>();
			DynamicTemplates = new ConcurrentDictionary<string, string>();
			AdditionalMetadataReferences = new HashSet<MetadataReference>();
			ExcludedAssemblies = new HashSet<string>();
			PreRenderCallbacks = new List<Action<ITemplatePage>>();
			DisableEncoding = false;
		}

		public ISet<string> Namespaces { get; set; }

		public IDictionary<string, string> DynamicTemplates { get; set; }

		public HashSet<MetadataReference> AdditionalMetadataReferences { get; set; }

		public HashSet<string> ExcludedAssemblies { get; set; }

		public virtual IList<Action<ITemplatePage>> PreRenderCallbacks { get; set; }

		public ICachingProvider CachingProvider { get; set; }

		/// <summary>
		/// Settings this to <c>true</c> will disable HTML encoding in all templates.
		/// It can be reenabled by setting <c>DisableEncoding = false</c> in the
		/// template.
		/// </summary>
		public bool DisableEncoding { get; set; }
	}
}
