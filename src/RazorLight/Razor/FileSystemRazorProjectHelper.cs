using System.Text;

namespace RazorLight.Razor
{
	public static class FileSystemRazorProjectHelper
	{
		public static string NormalizeKey(string templateKey)
		{
			var addLeadingSlash = templateKey[0] != '\\' && templateKey[0] != '/';
			var transformSlashes = templateKey.IndexOf('\\') != -1;

			if (!addLeadingSlash && !transformSlashes)
			{
				return templateKey;
			}

			var length = templateKey.Length;
			if (addLeadingSlash)
			{
				length++;
			}

			var builder = new StringBuilder(length);
			if (addLeadingSlash)
			{
				builder.Append('/');
			}

			for (var i = 0; i < templateKey.Length; i++)
			{
				var ch = templateKey[i];
				if (ch == '\\')
				{
					ch = '/';
				}
				builder.Append(ch);
			}

			return builder.ToString();
		}
	}
}
