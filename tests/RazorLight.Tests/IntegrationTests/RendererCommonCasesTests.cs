using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

namespace RazorLight.Tests.IntegrationTests
{
	public class TestViewModel
	{
		public string Name { get; set; }

		public int NumberOfItems { get; set; }
	}

	public class RendererCommonCasesTests :
		VerifyBase
	{
		[Fact]
		public async Task Should_Render_Section_And_ViewModel()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel {Name = "RazorLight", NumberOfItems = 100};
			var renderedResult = await engine.CompileRenderAsync("template4.cshtml", model);
			await Verify(renderedResult);
		}

		[Fact]
		public async Task Should_Render_Sections_With_IncludeAsync()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel {Name = "RazorLight", NumberOfItems = 200};
			var renderedResult = await engine.CompileRenderAsync("template5.cshtml", model);
			await Verify(renderedResult);
		}

		[Fact]
		public async Task Should_Fail_When_Required_Section_Is_Missing()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel {Name = "RazorLight", NumberOfItems = 300};
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await engine.CompileRenderAsync("template6.cshtml", model));
			await Verify(exception);
		}

		[Fact]
		public async Task Should_Render_IncludeAsync()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel {Name = "RazorLight", NumberOfItems = 400};
			var renderedResult = await engine.CompileRenderAsync("template7.cshtml", model);
			await Verify(renderedResult);
		}

		[Fact]
		public async Task Should_Render_Nested_IncludeAsync()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel {Name = "RazorLight", NumberOfItems = 400};
			var renderedResult = await engine.CompileRenderAsync("template9.cshtml", model);
			await Verify(renderedResult);
		}

		[Fact]
		public async Task Should_Render_RequiredSections_That_Have_Nested_IncludeAsync()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel {Name = "RazorLight", NumberOfItems = 400};
			var renderedResult = await engine.CompileRenderAsync("template8.cshtml", model);
			await Verify(renderedResult);
		}

		public RendererCommonCasesTests(ITestOutputHelper output) :
			base(output)
		{
		}
	}
}