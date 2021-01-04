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
			string location = assembly.Location;
			UriBuilder uri = new UriBuilder(location);
			return Uri.UnescapeDataString(uri.Path);
		}
	}
}
