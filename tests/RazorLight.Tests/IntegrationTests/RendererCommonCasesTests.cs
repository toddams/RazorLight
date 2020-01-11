using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Snapper;
using Snapper.Attributes;
using Xunit;

namespace RazorLight.Tests.IntegrationTests
{
	public class TestViewModel
	{
		public string Name { get; set; }

		public int NumberOfItems { get; set; }
	}
	
	[UpdateSnapshots]
	public class RendererCommonCasesTests
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
			Assert.NotNull(renderedResult);
			renderedResult.ShouldMatchSnapshot();
		}
		
		[Fact]
		public async Task Should_Render_Sections_With_IncludeAsync()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); 
			
			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel {Name = "RazorLight", NumberOfItems = 200 };
			var renderedResult = await engine.CompileRenderAsync("template5.cshtml", model);
			Assert.NotNull(renderedResult);
			renderedResult.ShouldMatchSnapshot();
		}		
		
		[Fact]
		public async Task Should_Fail_When_Required_Section_Is_Missing()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); 
			
			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel {Name = "RazorLight", NumberOfItems = 300};
			var renderedResult = await engine.CompileRenderAsync("template6.cshtml", model);
			Assert.NotNull(renderedResult);
			renderedResult.ShouldMatchSnapshot();
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
			Assert.NotNull(renderedResult);
			renderedResult.ShouldMatchSnapshot();
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
			Assert.NotNull(renderedResult);
			renderedResult.ShouldMatchSnapshot();
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
			Assert.NotNull(renderedResult);
			renderedResult.ShouldMatchSnapshot();
		}		
	}
}