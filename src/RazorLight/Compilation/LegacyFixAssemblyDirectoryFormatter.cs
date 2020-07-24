using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RazorLight.Compilation
{
	public class LegacyFixAssemblyDirectoryFormatter : IAssemblyDirectoryFormatter
	{
		public string GetAssemblyDirectory(Assembly assembly)
		{
			string codeBase = assembly.CodeBase;
			UriBuilder uriBuilder = new UriBuilder(codeBase);
			string assemlbyDirectory = Uri.UnescapeDataString(uriBuilder.Path + uriBuilder.Fragment);
			return assemlbyDirectory;
		}
	}
}
