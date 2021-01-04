using System.IO;
using System.Reflection;

namespace RazorLight.Tests.Utils
{
	public static class DirectoryUtils
	{
		public static string RootDirectory
		{
			get
			{
				var location = typeof(DirectoryUtils).GetTypeInfo().Assembly.Location;
				if (!File.Exists(location)) throw new FileNotFoundException($"Could not find file location [{location}]");
				location = Directory.GetParent(location).FullName;
				if (!Directory.Exists(location)) throw new DirectoryNotFoundException($"Could not find location [{location}].");
				return location;
			}
		}
	}
}
