using System;
using System.IO;

namespace RazorLight.Templating
{
	public class LoadedTemplateSource : ITemplateSource
	{
		public string Content { get; private set; }
		public string FilePath { get; private set; }
		public string TemplateKey { get; }

		public LoadedTemplateSource(string content) : this(GetRandomString(), content)
		{
		}

		public LoadedTemplateSource(string key, string content)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (string.IsNullOrEmpty(content))
			{
				throw new ArgumentNullException(nameof(content));
			}

			this.Content = content;
			this.TemplateKey = key;
			this.FilePath = null;
		}

		private static string GetRandomString()
		{
			return Path.GetFileName(Path.GetRandomFileName());
		}

		public TextReader CreateReader()
		{
			return new StringReader(Content);
		}
	}
}
