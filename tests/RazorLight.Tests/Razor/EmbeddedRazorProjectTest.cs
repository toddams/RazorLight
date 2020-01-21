using RazorLight.Razor;
using System;
using System.IO;
using System.Threading.Tasks;
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
		public void Assembly_IsApplied_OnConstructor_FromRootType()
		{
			var type = typeof(EmbeddedRazorProject);

			var project = new EmbeddedRazorProject(type);

			Assert.NotNull(project.Assembly);
			Assert.Equal(project.Assembly, type.Assembly);
		}

		[Fact]
		public async Task Ensure_TemplateKey_IsNormalizedAsync()
		{
			var project = new EmbeddedRazorProject(typeof(Root));

			var item = await project.GetItemAsync(EMPTY_TEMPLATE);

			Assert.NotNull(item);
			Assert.Equal(item.Key, EMPTY_TEMPLATE + project.Extension);
		}
	}
}
