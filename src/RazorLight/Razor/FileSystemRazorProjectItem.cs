using System;
using System.IO;

namespace RazorLight.Razor
{
	public class FileSystemRazorProjectItem : RazorLightProjectItem
	{
		public FileSystemRazorProjectItem(string templateKey, FileInfo fileInfo)
		{
			Key = templateKey ?? throw new ArgumentNullException(nameof(templateKey));
			File = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
		}

		public FileInfo File { get; }

		public override string Key { get; }

		public override bool Exists => File.Exists;

		public override Stream Read() => File.OpenRead();
	}
}
