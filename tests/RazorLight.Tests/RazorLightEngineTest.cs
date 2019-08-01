namespace RazorLight.Tests
{
	using System.Threading.Tasks;
	using Xunit;

	public class RazorLightEngineTest
	{

		[Fact]
		public async Task Ensure_Option_DisablEncoding_Renders_Models_Raw()
		{
			//Assing
			var engine = new RazorLightEngineBuilder()
				.UseMemoryCachingProvider()
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

		//TODO: add string rendering test

		//      [Fact]
		//public async Task Ensure_Content_Added_To_DynamicTemplates()
		//{
		//	var options = new RazorLightOptions();

		//	string key = "key";
		//	string content = "content";
		//	var project = new TestRazorProject();
		//	project.Value = new TextSourceRazorProjectItem(key, content);

		//	var engine = new EngineFactory().Create(project, options);

		//	await engine.CompileRenderAsync(key, content, new object(), typeof(object));

		//	Assert.NotEmpty(options.DynamicTemplates);
		//	Assert.Contains(options.DynamicTemplates, t => t.Key == key && t.Value == content);
		//}
	}
}
