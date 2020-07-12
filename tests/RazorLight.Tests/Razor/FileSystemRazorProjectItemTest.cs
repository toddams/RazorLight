using RazorLight.Razor;
using System;
using System.IO;
using Xunit;

namespace RazorLight.Tests.Razor
{
	public class FileSystemRazorProjectItemTest
	{
		[Fact]
		public void Throws_OnConstructor_NullParams()
		{
			Assert.Throws<ArgumentNullException>(() => new FileSystemRazorProjectItem(null, new FileInfo("C:/")));
			Assert.Throws<ArgumentNullException>(() => new FileSystemRazorProjectItem("key", null));
		}

		[Fact]
		public void Ensure_ConstructorParams_AreApplied()
		{
			string templateKey = "Assets/Files/Empty.cshtml";
			var fileInfo = new FileInfo(Path.Combine(DirectoryUtils.RootDirectory, templateKey));

			var item = new FileSystemRazorProjectItem(templateKey, fileInfo);

			Assert.NotNull(item);
			Assert.Equal(item.Key, templateKey);
			Assert.Equal(item.File, fileInfo);
		}

		[Fact]
		public void ReturnsExistTrue_OnExistingTemplate()
		{
			string templateKey = "Assets/Files/Empty.cshtml";
			var fileInfo = new FileInfo(Path.Combine(DirectoryUtils.RootDirectory, templateKey));

			var item = new FileSystemRazorProjectItem(templateKey, fileInfo);

			Assert.True(item.Exists);
		}

		[Fact]
		public void ReturnsExistFalse_OnExistingTemplate()
		{
			string templateKey = "Assets/Files/IDoNotExist.cshtml";
			var fileInfo = new FileInfo(Path.Combine(DirectoryUtils.RootDirectory, templateKey));

			var item = new FileSystemRazorProjectItem(templateKey, fileInfo);

			Assert.False(item.Exists);
		}

		[Fact]
		public void Read_ReturnsFileContent()
		{
			string templateKey = "Assets/Files/Empty.cshtml";
			string templatePath = Path.Combine(DirectoryUtils.RootDirectory, templateKey);
			string fileContent = File.ReadAllText(templatePath);

			var item = new FileSystemRazorProjectItem(templateKey, new FileInfo(templatePath));
			string itemContent = null;

			using (var reader = new StreamReader(item.Read()))
			{
				itemContent = reader.ReadToEnd();
			}

			Assert.NotNull(itemContent);
			Assert.Equal(fileContent, itemContent);
		}
	}
}
