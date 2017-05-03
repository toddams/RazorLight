using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;

namespace RazorLight.Templating.FileSystem
{
	public class FilesystemTemplateManager : ITemplateManager, IDisposable
	{
		private readonly PhysicalFileProvider fileProvider;

		public FilesystemTemplateManager(string root)
		{
			if (string.IsNullOrEmpty(root))
			{
				throw new ArgumentNullException(nameof(root));
			}

			this.Root = root;
			this.fileProvider = new PhysicalFileProvider(root);
		}

		public string Root { get; }

		public ITemplateSource Resolve(string key)
		{
			IFileInfo fileInfo = GetFileInfo(key);

			FileTemplateSource source = new FileTemplateSource(fileInfo, key);

			return source;
		}

		private IFileInfo GetFileInfo(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			List<string> patterns = new List<string> {
				key,
				$"Shared/{key}"
			};
			if (key.StartsWith("/Views/")) patterns.Add(key.Substring("/Views/".Length));
			var fileInfos = patterns.Select(p => fileProvider.GetFileInfo(p));
			IFileInfo fileInfo = (from p in fileInfos
								  where p.Exists
								  select p).FirstOrDefault() ?? fileInfos.FirstOrDefault();
			if (!fileInfo.Exists || fileInfo.IsDirectory)
			{
				throw new FileNotFoundException("Can't find a file with a specified key", key);
			}

			return fileInfo;
		}

		public void Dispose()
		{
			if (fileProvider != null)
			{
				fileProvider.Dispose();
			}
		}
	}
}
