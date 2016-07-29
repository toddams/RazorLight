using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace RazorLight
{
	public class ConfigurationOptions
	{
		public string ViewsFolder { get; set; }

		/// <summary>
		/// If set to true - all dependencies from the entry assembly will be added as a compiler metatada references while compiling Razor views
		/// </summary>
		public bool LoadDependenciesFromEntryAssembly { get; set; } = true;

		/// <summary>
		/// Additional compilation metadata referenes
		/// </summary>
		public IList<MetadataReference> AdditionalCompilationReferences { get; } = new List<MetadataReference>();

		public static ConfigurationOptions Default => new ConfigurationOptions();
    }
}
