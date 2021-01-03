using RazorLight.Tests.Utils;

namespace RazorLight.Tests
{
	using System.Threading.Tasks;
	using Xunit;

	public class RazorLightEngineTest
	{

		[Fact]
		public async Task Ensure_Option_Disable_Encoding_Renders_Models_Raw()
		{
			//Arrange
			var engine = new RazorLightEngineBuilder()
				.UseMemoryCachingProvider()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseFileSystemProject(DirectoryUtils.RootDirectory)
				.DisableEncoding()
				.Build();

			string key = "key";
			string content = "@Model.Entity";

			var model = new { Entity = "<pre></pre>" };

			// act
			var result = await engine.CompileRenderStringAsync(key, content, model);

			// assert
			Assert.Contains("<pre></pre>", result);
		}

		[Fact]
		public async Task Ensure_QuickStart_Demo_Code_Works()
		{
			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseEmbeddedResourcesProject(typeof(Root))
				.UseMemoryCachingProvider()
				.Build();

			string template = "Hello, @Model.Name. Welcome to RazorLight repository";
			var model = new { Name = "John Doe" };

			string result = await engine.CompileRenderStringAsync("templateKey", template, model);
			Assert.Equal("Hello, John Doe. Welcome to RazorLight repository", result);
		}

		//[Fact]
		//public async Task Ensure_Content_Added_To_DynamicTemplates()
		//{
		//	var options = new RazorLightOptions();

		//	string key = "key";
		//	string content = "content";
		//	var project = new TestRazorProject();
		//	project.Value = new TextSourceRazorProjectItem(key, content);

		//	var engine = new RazorLightEngineBuilder()
		//		.UseProject(project)
		//		.Build();

		//	await engine.CompileRenderStringAsync(key, content, new object(), new ExpandoObject());

		//	Assert.NotEmpty(options.DynamicTemplates);
		//	Assert.Contains(options.DynamicTemplates, t => t.Key == key && t.Value == content);
		//}
	}
}
