using RazorLight.Razor;
using System;
using System.Linq;
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

		[Fact]
		public async Task Ensure_GetKnownKeysAsync_Returns_Expected_Keys()
		{
			var subsetToCheck = new[]
			{
				"Assets.Embedded.Empty",
				"Assets.Embedded.Layout"
			};

			var knownKeys = await new EmbeddedRazorProject(typeof(Root)).GetKnownKeysAsync();

			foreach (var key in subsetToCheck)
			{
				Assert.Contains(key, knownKeys);
			}
		}

		[Fact]
		public async Task Ensure_GetKnownKeysAsync_Returns_Expected_Keys_When_RootNamespace_Set()
		{
			var subsetToCheck = new[]
			{
				"Empty",
				"Layout"
			};

			var knownKeys = await new EmbeddedRazorProject(typeof(Root).Assembly, "RazorLight.Tests.Assets.Embedded")
				.GetKnownKeysAsync();

			foreach (var key in subsetToCheck)
			{
				Assert.Contains(key, knownKeys);
			}
		}

		[Fact]
		public async Task Ensure_GetKnownKeysAsync_Returns_Existing_Keys()
		{
			var project = new EmbeddedRazorProject(typeof(Root));

			var knownKeys = await project.GetKnownKeysAsync();
			Assert.NotNull(knownKeys);
			Assert.True(knownKeys.Count() > 0);

			foreach (var key in knownKeys)
			{
				var projectItem = await project.GetItemAsync(key);
				Assert.True(projectItem.Exists);
			}
		}

		[Fact]
		public async Task Ensure_GetKnownKeysAsync_Returns_Existing_Keys_When_RootNamespace_Set()
		{
			var project = new EmbeddedRazorProject(typeof(Root).Assembly, "RazorLight.Tests.Assets.Embedded");

			var knownKeys = await project.GetKnownKeysAsync();
			Assert.NotNull(knownKeys);
			Assert.True(knownKeys.Count() > 0);

			foreach (var key in knownKeys)
			{
				var projectItem = await project.GetItemAsync(key);
				Assert.True(projectItem.Exists);
			}
		}
	}
}
