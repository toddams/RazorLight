using RazorLight.Razor;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace RazorLight.Tests.Razor
{
    public class FileSystemRazorProjectTest
    {
        [Fact]
        public void NotExiting_RootDirectory_Throws()
        {
            Action action = () => new FileSystemRazorProject(@"C:/Not/Existing/Folder/Here");

            Assert.Throws<DirectoryNotFoundException>(action);
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
    }
}
