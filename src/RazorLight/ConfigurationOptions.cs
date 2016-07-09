using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace RazorLight
{
	public class ConfigurationOptions
    {
		public bool LoadDependenciesFromEntryAssembly { get; set; } = true;

		public IList<MetadataReference> AdditionalCompilationReferences { get; } = new List<MetadataReference>();

		public static ConfigurationOptions Default => new ConfigurationOptions();
    }
}
