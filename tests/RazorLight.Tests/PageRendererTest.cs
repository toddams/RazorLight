using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using Moq;
using RazorLight.Caching;
using RazorLight.Rendering;
using RazorLight.Templating;
using Xunit;

namespace RazorLight.Tests
{
	public class PageRendererTest
	{
		[Fact]
		public void Template_Shares_Context_With_Layout()
		{
			string expected = "Begin Layout" +
							  "Hello" +
							  "Begin" +
							  "Hello" +
							  "End" +
							  "End Layout";

			var layout = TemplatePageTest.CreatePage(v =>
			{
				v.Write("Begin Layout");
				v.Write(v.ViewBag.Title);
				v.Write(v.RenderBodyPublic());
				v.Write("End Layout");
			});

			var page = TemplatePageTest.CreatePage(v =>
			{
				v.Write("Begin");
				v.Write(v.ViewBag.Title);
				v.Write("End");
			});

			var lookup = new Mock<IPageLookup>();
			lookup.Setup(p => p.GetPage(It.IsAny<string>()))
				.Returns(new PageLookupResult(
					new PageLookupItem(It.IsAny<string>(), () => layout), new List<PageLookupItem>()));

			dynamic viewbag = new ExpandoObject();
			viewbag.Title = "Hello";
			var context = new PageContext(viewbag);

			page.Layout = "_";
			page.PageContext = context;

			string output;

			using (var writer = new StringWriter())
			{
				page.PageContext.Writer = writer;
				var renderer = new PageRenderer(page, lookup.Object);
				renderer.RenderAsync(context).Wait();

				output = writer.ToString();
			}

			Assert.Equal(expected, output, StringComparer.Ordinal);
		}

		[Fact]
		public void Ensure_Callbacks_Exeuted_In_Correct_Order()
		{
			var page = TemplatePageTest.CreatePage(v => { });
			var callbackRecords = new List<string>();

			var context = new PageContext();

			context.PrerenderCallbacks.Add((t) =>
			{
				callbackRecords.Add("PAGE_SPECIFIC_CALLBACK");
			});

			page.PageContext = context;

			var renderer = new PageRenderer(page, CreateLookupForPageInstance(page));
			renderer.PreRenderCallbacks.Add(new Action<TemplatePage>((t) =>
			{
				callbackRecords.Add("GLOBAL_CALLBACK");
			}));

			using (var writer = new StringWriter())
			{
				page.PageContext.Writer = writer;
				renderer.RenderAsync(context).Wait();
			}

			Assert.True(callbackRecords.Count == 2);
			Assert.Equal(callbackRecords[0], "PAGE_SPECIFIC_CALLBACK");
			Assert.Equal(callbackRecords[1], "GLOBAL_CALLBACK");
		}

		private IPageLookup CreateLookupForPageInstance(TemplatePage page)
		{
			var lookup = new Mock<IPageLookup>();
			lookup.Setup(p => p.GetPage(It.IsAny<string>()))
				.Returns(new PageLookupResult(
					new PageLookupItem(It.IsAny<string>(), () => page), new List<PageLookupItem>()));

			return lookup.Object;
		}
	}
}
