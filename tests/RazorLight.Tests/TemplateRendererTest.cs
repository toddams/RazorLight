using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Moq;
using RazorLight.Internal;
using Xunit;

namespace RazorLight.Tests
{
	public class TemplateRendererTest
	{
		[Fact]
		public async Task Ensure_PrerenderCallbacks_Are_Invoked()
		{
			//Assign
			var page = TemplatePageTest.CreatePage((t) => t.Write("test"));

			bool triggered1 = false, triggered2 = false;
			var callbacks = new List<Action<ITemplatePage>>()
			{
				(t) => triggered1 = true,
				(t) => triggered2 = true
			};

			var options = new RazorLightOptions() { PreRenderCallbacks = callbacks };
			var engineMock = new Mock<IEngineHandler>();
			engineMock.SetupGet(e => e.Options).Returns(options);

			//Act
			var templateRenderer = new TemplateRenderer(page, engineMock.Object, HtmlEncoder.Default, new MemoryPoolViewBufferScope());
			await templateRenderer.RenderAsync();

			//Assert
			Assert.True(triggered1);
			Assert.True(triggered2);
		}

		[Fact]
		public async Task Template_Shares_Context_With_Layout()
		{
			var encoder = new TemplatePageTest.HtmlTestEncoder();

			string expected = encoder.Encode("Begin Layout") +
							  encoder.Encode("Hello") +
							  encoder.Encode("Begin") +
							  encoder.Encode("Hello") +
							  encoder.Encode("End") +
							  encoder.Encode("End Layout");

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

			var engineMock = new Mock<IEngineHandler>();
			engineMock.Setup(t => t.CompileTemplateAsync(It.IsAny<string>()))
				.Returns(new Func<Task<ITemplatePage>>(() => { return Task.FromResult((ITemplatePage)layout); }));

			engineMock.SetupGet(t => t.Options).Returns(new RazorLightOptions());

			dynamic viewbag = new ExpandoObject();
			viewbag.Title = "Hello";
			var context = new PageContext(viewbag);

			page.Layout = "_";
			page.PageContext = context;

			string output;

			using (var writer = new StringWriter())
			{
				page.PageContext.Writer = writer;
				var renderer = new TemplateRenderer(page, engineMock.Object, encoder, new MemoryPoolViewBufferScope());
				await renderer.RenderAsync();

				output = writer.ToString();
			}

			Assert.Equal(expected, output, StringComparer.Ordinal);
		}

		[Fact]
		public async Task Template_Shares_Model_With_Layout()
		{
			var engine = new RazorLightEngineBuilder()
				.UseEmbeddedResourcesProject(typeof(Root).Assembly, "Assets.Embedded")
				.Build();

			var model = new TestModel()
			{
				Value = "123"
			};

			string expected = $"Layout: {model.Value}_body: {model.Value}";

			string result = await engine.CompileRenderAsync("WithModelAndLayout", model);
			result = result.Replace(Environment.NewLine, "");

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task Templates_Supports_Local_Functions()
		{
			// See https://github.com/aspnet/Razor/issues/715

			var engine = new RazorLightEngineBuilder()
				.UseEmbeddedResourcesProject(typeof(Root).Assembly, "Assets.Embedded")
				.Build();

			string expected = "<strong>LocalFunction</strong>";

			string result = await engine.CompileRenderAsync("LocalFunction", (object)null);
			result = result.Replace(Environment.NewLine, "");

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task Templates_Supports_Local_Functions_Using_Helper()
		{
			// See https://github.com/aspnet/Razor/issues/715

			var engine = new RazorLightEngineBuilder()
				.UseEmbeddedResourcesProject(typeof(Root).Assembly, "Assets.Embedded")
				.Build();

			string expected = "<strong>LocalFunctionUsingHelper</strong>";

			string result = await engine.CompileRenderAsync("LocalFunctionUsingHelper", (object)null);
			result = result.Replace(Environment.NewLine, "");

			Assert.Equal(expected, result);
		}

		[Fact]
		public async Task Templates_Supports_Conditional_Attribute_Rendering()
		{
			// https://github.com/aspnet/AspNetCore/issues/5076
			var engine = new RazorLightEngineBuilder()
				.UseEmbeddedResourcesProject(typeof(Root).Assembly, "Assets.Embedded")
				.Build();

			string expected = "<strong attr=\"class=\"Conditional Attribute\"\"></strong>";

			var result = await engine.CompileRenderAsync("ConditionalAttributeRendering",true);

			Assert.Contains(expected, result);
		}

		public class TestModel
		{
			public string Value { get; set; }
		}
	}
}
