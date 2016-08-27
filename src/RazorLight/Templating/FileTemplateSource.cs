using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using RazorLight.Internal;

namespace RazorLight.Templating
{
	public class FileTemplateSource : ITemplateSource
	{
		private string template;

		public FileTemplateSource(IFileInfo fileInfo, string relativeFilePath)
		{
			if (fileInfo == null)
			{
				throw new ArgumentNullException(nameof(fileInfo));
			}

			if (string.IsNullOrEmpty(relativeFilePath))
			{
				throw new ArgumentNullException(nameof(relativeFilePath));
			}

			this.FileInfo = fileInfo;
			this.FilePath = PathNormalizer.GetNormalizedPath(fileInfo.PhysicalPath);
			this.TemplateKey = relativeFilePath;
		}

		public string Content
		{
			get
			{
				if (string.IsNullOrEmpty(template))
				{
					using (var reader = CreateReader())
					{
						template = reader.ReadToEnd();
					}
				}

				return template;
			}
		}
		public string FilePath { get; }
		public string TemplateKey { get; }

		public IFileInfo FileInfo { get; private set; }

		public TextReader CreateReader()
		{
			return new StreamReader(FileInfo.CreateReadStream());
		}
	}
}

