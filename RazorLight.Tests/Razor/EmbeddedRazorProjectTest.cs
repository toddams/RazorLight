using RazorLight.Razor;
using System;
using System.IO;
using Xunit;

namespace RazorLight.Tests.Razor
{
    public class EmbeddedRazorProjectTest
    {
        private const string EMPTY_TEMPLATE = "Assets.Embedded.Empty";

        [Fact]
        public void Ensure_Throws_OnNullRootType()
        {
            Assert.Throws<ArgumentNullException>(() => { new EmbeddedRazorProject(null); });
        }

        [Fact]
        public void Ensure_Throws_OnNullTemplateKey()
        {
            var project = new EmbeddedRazorProject(typeof(EmbeddedRazorProject));

            Assert.ThrowsAsync<ArgumentNullException>(async () => { await project.GetItemAsync(null); });
        }

        [Fact]
        public void RootType_IsApplied_OnConstructor()
        {
            var type = typeof(EmbeddedRazorProject);

            var project = new EmbeddedRazorProject(type);

            Assert.NotNull(project.RootType);
            Assert.Equal(project.RootType, type);
        }

        [Fact]
        public async System.Threading.Tasks.Task Ensure_TemplateKey_IsNormalizedAsync()
        {
            var project = new EmbeddedRazorProject(typeof(Root));

            var item = await project.GetItemAsync(EMPTY_TEMPLATE);

            Assert.NotNull(item);
            Assert.Equal(item.Key, EMPTY_TEMPLATE + project.Extension);
        }
    }
}
