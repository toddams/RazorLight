using System;
using System.Reflection;

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
