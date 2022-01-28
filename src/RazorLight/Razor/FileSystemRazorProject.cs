using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RazorLight.Razor
{
	/// <summary>
	/// Specifies RazorProject where templates are located in files
	/// </summary>
	public class FileSystemRazorProject : RazorLightProject
	{
		public const string DefaultExtension = ".cshtml";
		private readonly IFileProvider _fileProvider;

		public FileSystemRazorProject(string root)
			: this(root, DefaultExtension)
		{
		}

		public FileSystemRazorProject(string root, string extension)
		{
			Extension = extension ?? throw new ArgumentNullException(nameof(extension));

			if (!Directory.Exists(root))
			{
				throw new DirectoryNotFoundException($"Root directory {root} not found");
			}

			Root = root;
			_fileProvider = new PhysicalFileProvider(Root);
		}

		public string Extension { get; set; }

		/// <summary>
		/// Looks up for the template source with a given <paramref name="templateKey" />
		/// </summary>
		/// <param name="templateKey">Unique template key</param>
		/// <returns></returns>
		public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
		{
			if (!templateKey.EndsWith(Extension))
			{
				templateKey = templateKey + Extension;
			}

			string absolutePath = NormalizeKey(templateKey);
			var item = new FileSystemRazorProjectItem(templateKey, new FileInfo(absolutePath));

			if (item.Exists)
			{
				item.ExpirationToken = _fileProvider.Watch(templateKey);
			}

			return Task.FromResult((RazorLightProjectItem)item);
		}

		/// <summary>
		/// Root folder
		/// </summary>
		public string Root { get; }

		protected string NormalizeKey(string templateKey)
		{
			if (string.IsNullOrEmpty(templateKey))
			{
				throw new ArgumentNullException(nameof(templateKey));
			}

			var absolutePath = templateKey;
			if (!absolutePath.StartsWith(Root, StringComparison.OrdinalIgnoreCase))
			{
				if (templateKey[0] == '/' || templateKey[0] == '\\')
				{
					templateKey = templateKey.Substring(1);
				}

				absolutePath = Path.Combine(Root, templateKey);
			}

			absolutePath = absolutePath.Replace('\\', '/');

			return absolutePath;
		}

		public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
		{
			return Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
		}
		public override Task<IEnumerable<string>> GetKnownKeysAsync()
		{
			var files = Directory.EnumerateFiles(Root, $"*{Extension}", SearchOption.AllDirectories)
				.Where(x => x.StartsWith(Root))
				.Select(x => x.Substring(Root.Length, x.Length - Root.Length))
				.Select(x => x.StartsWith("\\") || x.StartsWith("/") ? x.Substring(1) : x)
				.Select(x => x.Replace('\\', '/'));

			return Task.FromResult(files);
		}
	}
}
