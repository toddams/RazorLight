﻿using RazorLight.Razor;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RazorLight.Tests.Utils;
using Xunit;

namespace RazorLight.Tests.Razor
{
	public class FileSystemRazorProjectTest
	{
		[Fact]
		public void NotExiting_RootDirectory_Throws()
		{
			void Action() => _ = new FileSystemRazorProject(@"C:/Not/Existing/Folder/Here");

			Assert.Throws<DirectoryNotFoundException>(Action);
		}

		[Fact]
		public void Ensure_RootProperty_AssignedOnConstructor()
		{
			string root = Path.Combine(DirectoryUtils.RootDirectory, "Assets", "Files");

			var project = new FileSystemRazorProject(root);

			Assert.Equal(project.Root, root);
		}

		[Fact]
		public void Ensure_ExtensionProperty_IsDefaultIfNotProvided()
		{
			string root = Path.Combine(DirectoryUtils.RootDirectory, "Assets", "Files");

			var project = new FileSystemRazorProject(root);

			Assert.Equal(project.Extension, FileSystemRazorProject.DefaultExtension);
		}

		[Fact]
		public void Ensure_ExtensionProperty_AssignedOnConstructor()
		{
			string root = Path.Combine(DirectoryUtils.RootDirectory, "Assets", "Files");
			string extension = FileSystemRazorProject.DefaultExtension + "_test";

			var project = new FileSystemRazorProject(root, extension);

			Assert.Equal(project.Extension, extension);
		}

		[Fact]
		public void Null_TemplateKey_ThrowsOn_GetItem()
		{
			var project = new FileSystemRazorProject(DirectoryUtils.RootDirectory);

			Assert.ThrowsAsync<ArgumentNullException>(async () => await project.GetItemAsync("not-existing-key"));
		}

		[Fact]
		public async Task Ensure_TemplateKey_IsNormalizedAsync()
		{
			var project = new FileSystemRazorProject(DirectoryUtils.RootDirectory);

			string templateKey = "Empty";

			var item = await project.GetItemAsync(Path.Combine("Assets", "Embedded", templateKey));

			Assert.NotNull(item);
			Assert.EndsWith(templateKey + project.Extension, item.Key);
		}

		[Fact]
		public async Task Ensure_GetKnownKeysAsync_Returns_Existing_Keys()
		{
			var project = new FileSystemRazorProject(DirectoryUtils.RootDirectory);

			var knownKeys = (await project.GetKnownKeysAsync()).ToList();
			Assert.NotNull(knownKeys);
			Assert.NotEmpty(knownKeys);

			foreach (var key in knownKeys)
			{
				var projectItem = await project.GetItemAsync(key);
				Assert.True(projectItem.Exists);
			}
		}

		[Fact]
		public async Task Ensure_GetKnownKeysAsync_Returns_Expected_Keys()
		{
			var subsetToCheck = new[]
			{
				"Assets/Files/Empty.cshtml",
				"Assets/Files/Layout.cshtml"
			};

			var project = new FileSystemRazorProject(DirectoryUtils.RootDirectory);

			var knownKeys = (await project.GetKnownKeysAsync()).ToList();
			Assert.NotNull(knownKeys);
			Assert.NotEmpty(knownKeys);

			foreach (var key in subsetToCheck)
			{
				Assert.Contains(key, knownKeys);
			}
		}
	}
}
