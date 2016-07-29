using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileProviders;

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

		public IFileProvider ViewsFileProvider
		{
			get
			{
				if (!String.IsNullOrEmpty(ViewsFolder))
					return new PhysicalFileProvider(ViewsFolder);
				else
					return new NullFileProvider();
			}
		}

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
