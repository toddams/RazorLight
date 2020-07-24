using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RazorLight.Compilation
{
	public class DefaultAssemblyDirectoryFormatter : IAssemblyDirectoryFormatter
	{
		public string GetAssemblyDirectory(Assembly assembly)
		{
			string codeBase = assembly.CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			return Uri.UnescapeDataString(uri.Path);
		}
	}
}
