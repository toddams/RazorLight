using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RazorLight;
using Xunit;

public class Snippets
{
	public class ViewModel
	{
		public string Name { get; set; }
	}

	[Fact]
	public async Task Simple()
	{
		#region Simple
		var engine = new RazorLightEngineBuilder()
			// required to have a default RazorLightProject type,
			// but not required to create a template from string.
			.UseEmbeddedResourcesProject(typeof(Program))
			.UseMemoryCachingProvider()
			.Build();

		string template = "Hello, @Model.Name. Welcome to RazorLight repository";
		ViewModel model = new ViewModel {Name = "John Doe"};

		string result = await engine.CompileRenderStringAsync("templateKey", template, model);

		#endregion

		Assert.NotNull(result);
	}

	public async Task RenderCompiledTemplate(RazorLightEngine engine, object model)
	{
		#region RenderCompiledTemplate
		var cacheResult = engine.Handler.Cache.RetrieveTemplate("templateKey");
		if(cacheResult.Success)
		{
			var templatePage = cacheResult.Template.TemplatePageFactory();
			string result = await engine.RenderTemplateAsync(templatePage, model);
		}
		#endregion
	}

	async Task FileSource()
	{
		#region FileSource
		var engine = new RazorLightEngineBuilder()
			.UseFileSystemProject("C:/RootFolder/With/YourTemplates")
			.UseMemoryCachingProvider()
			.Build();

		var model = new {Name = "John Doe"};
		string result = await engine.CompileRenderAsync("Subfolder/View.cshtml", model);

		#endregion
	}

	async Task EmbeddedResourceSource()
	{
		#region EmbeddedResourceSource
		var engine = new RazorLightEngineBuilder()
			.UseEmbeddedResourcesProject(typeof(Program))
			.UseMemoryCachingProvider()
			.Build();

		var model = new SchoolForAnts();
		string result = await engine.CompileRenderAsync<object>("Views.Subfolder.SchoolForAnts", model, null);

		#endregion
	}

	public class SchoolForAnts
	{
	}
}