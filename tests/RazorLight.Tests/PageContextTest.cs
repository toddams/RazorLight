using System.Dynamic;
using Xunit;

namespace RazorLight.Tests
{
	public class PageContextTest
    {
		[Fact]
		public void Ensure_ViewBag_Is_Initialized()
		{
			var context = new PageContext();

			Assert.NotNull(context.ViewBag);
		}

		[Fact]
		public void Ensure_ViewStart_Collection_Is_Initialized()
		{
			var context = new PageContext();

			Assert.NotNull(context.ViewStartPages);
			Assert.Empty(context.ViewStartPages);
		}

		[Fact]
		public void Ensure_Passed_Viewbag_Is_Applied()
		{
			dynamic viewBag = new ExpandoObject();
			viewBag.Test = "test";

			var context = new PageContext(viewBag);

			Assert.NotNull(context.ViewBag);
			Assert.Same(context.ViewBag, viewBag);
		}
    }
}
