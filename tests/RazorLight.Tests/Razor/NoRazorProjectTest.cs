using RazorLight.Razor;
using System.Threading.Tasks;
using Xunit;

namespace RazorLight.Tests.Razor
{
	public class NoRazorProjectTest
	{
		[Fact]
		public async Task Ensure_GetImportsAsync_Returns_Empty_Enumerable()
		{
			var project = new NoRazorProject();

			var actual = await project.GetImportsAsync(null);

			Assert.Empty(actual);
		}

		[Fact]
		public async Task Ensure_GetItemAsync_Returns_NoRazorProjectItem()
		{
			var project = new NoRazorProject();

			var actual = await project.GetItemAsync(null);

			Assert.Equal(NoRazorProjectItem.Empty, actual);
		}

		[Fact]
		public async Task Ensure_GetKnownKeysAsync_Returns_Empty_Enumerable()
		{
			var project = new NoRazorProject();

			var actual = await project.GetKnownKeysAsync();

			Assert.Empty(actual);
		}
	}
}
