using System.Diagnostics;
using System.Text;

namespace RazorLight.Internal
{
	public static class PathNormalizer
	{
		public static string GetNormalizedPath(string relativePath)
		{
			Debug.Assert(relativePath != null);
			if (relativePath.Length == 0)
			{
				return relativePath;
			}

			var builder = new StringBuilder(relativePath);
			builder.Replace('\\', '/');

			return builder.ToString();
		}
	}
}
