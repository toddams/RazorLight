using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using RazorLight.Caching;
using System.Reflection;

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
		}

		public ISet<string> Namespaces { get; set; }

		public IDictionary<string, string> DynamicTemplates { get; set; }

		public HashSet<MetadataReference> AdditionalMetadataReferences { get; set; }

		public HashSet<string> ExcludedAssemblies { get; set; }

		public virtual IList<Action<ITemplatePage>> PreRenderCallbacks { get; set; }

		public ICachingProvider CachingProvider { get; set; }

		public Assembly OperatingAssembly { get; set; }

		/// <summary>
		/// Settings this to <c>true</c> will disable HTML encoding in all templates.
		/// It can be re-enabled by setting <c>DisableEncoding = false</c> in the
		/// template.
		/// </summary>
		public bool? DisableEncoding { get; set; }

		/// <summary>
		/// Setting this to <c>true</c> provides more information in exceptions.
		/// </summary>
		public bool? EnableDebugMode { get; set; }
	}
}
