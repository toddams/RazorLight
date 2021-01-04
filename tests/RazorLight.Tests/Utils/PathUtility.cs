using System.IO;
using System.Reflection;

namespace RazorLight.Tests.Utils
{
	public class PathUtility
	{
		public static string GetViewsPath()
		{
			string assemblyLocation = typeof(PathUtility).GetTypeInfo().Assembly.Location;
			string assemblyDir = Path.GetDirectoryName(assemblyLocation);

			return Path.Combine(assemblyDir, "Assets");
		}
	}
}
