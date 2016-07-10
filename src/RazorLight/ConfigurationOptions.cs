using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;

namespace RazorLight
{
	public class ConfigurationOptions
    {
		private string viewsFolder;
		public string ViewsFolder
		{
			get
			{
				return viewsFolder;
			}
			set
			{
				if (value == null || !Directory.Exists(value))
				{
					throw new DirectoryNotFoundException();
				}

				viewsFolder = value;
			}
		}

		public bool LoadDependenciesFromEntryAssembly { get; set; } = true;

		public IList<MetadataReference> AdditionalCompilationReferences { get; } = new List<MetadataReference>();

		public static ConfigurationOptions Default => new ConfigurationOptions();
    }
}
