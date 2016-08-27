using System;
using System.IO;
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

			IFileInfo fileInfo = fileProvider.GetFileInfo(key);
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
