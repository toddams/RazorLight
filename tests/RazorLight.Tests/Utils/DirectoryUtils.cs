using System;
using System.IO;

namespace RazorLight.Tests.Utils
{
	public static class DirectoryUtils
	{
		public static string RootDirectory
		{
			get
			{
				var location = AppContext.BaseDirectory;

				if (!Directory.Exists(location)) throw new DirectoryNotFoundException($"Could not find location [{location}].");
				return location;
			}
		}
	}
}
