﻿using System;
using System.Reflection;

namespace RazorLight.Compilation
{
	public class LegacyFixAssemblyDirectoryFormatter : IAssemblyDirectoryFormatter
	{
		public string GetAssemblyDirectory(Assembly assembly)
		{
			string codeBase = assembly.CodeBase;
			UriBuilder uriBuilder = new UriBuilder(codeBase);
			string assemblyDirectory = Uri.UnescapeDataString(uriBuilder.Path + uriBuilder.Fragment);
			return assemblyDirectory;
		}
	}
}
