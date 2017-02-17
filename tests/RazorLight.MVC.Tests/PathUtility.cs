using System;
using System.IO;
using System.Reflection;

namespace RazorLight.MVC.Tests
{
	public class PathUtility
    {
		public static string GetViewsPath()
		{
			//string isTesting = Environment.GetEnvironmentVariable("IS_TESTING_ENVIRONMENT");

			//if (!string.IsNullOrEmpty(isTesting) && isTesting.Equals("true"))
			//{
			//	return Path.Combine(Directory.GetCurrentDirectory(), "tests", "RazorLight.Tests", "Views");
			//}

			string assemblyLocation = typeof(PathUtility).GetTypeInfo().Assembly.Location;
			string assemblyDir = Path.GetDirectoryName(assemblyLocation);


			return Path.Combine(assemblyDir, "Views");
		}
	}
}
