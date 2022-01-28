using System.Collections.Generic;
using System.Dynamic;
using RazorLight.Compilation;
using RazorLight.Razor;
using RazorLight.Tests.Razor;
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

		[Fact]
		public async Task Ensure_Content_Added_To_DynamicTemplates()
		{
			var options = new RazorLightOptions();

			const string key = "key";
			const string content = "content";
			var project = new TestRazorProject {Value = new TextSourceRazorProjectItem(key, content)};

			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseProject(project)
				.UseOptions(options)
				.AddDynamicTemplates(new Dictionary<string, string>
				{
					[key] = content,
				})
				.Build();

			var actual = await engine.CompileRenderStringAsync(key, content, new object(), new ExpandoObject());

			Assert.NotEmpty(options.DynamicTemplates);
			Assert.Contains(options.DynamicTemplates, t => t.Key == key && t.Value == content);
			Assert.Equal(content, actual);
		}

		[Fact]
		public async Task Ensure_Content_Added_To_DynamicTemplates_When_Options_Not_Set_Explicitly()
		{
			const string key = "key";
			const string content = "content";
			var project = new NoRazorProject();

			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseProject(project)
				.AddDynamicTemplates(new Dictionary<string, string>
				{
					[key] = content,
				})
				.Build();

			var actual = await engine.CompileRenderStringAsync(key, content, new object(), new ExpandoObject());

			Assert.NotEmpty(engine.Options.DynamicTemplates);
			Assert.Contains(engine.Options.DynamicTemplates, t => t.Key == key && t.Value == content);
			Assert.Equal(content, actual);
		}

		[Fact]
		public async Task Ensure_Content_Added_To_DynamicTemplates_When_Both_RazorLightProject_And_Options_Not_Set_Explicitly()
		{
			const string key = "key";
			const string content = "content";

			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.Build();

			var actual = await engine.CompileRenderStringAsync(key, content, new object(), new ExpandoObject());

			Assert.NotEmpty(engine.Options.DynamicTemplates);
			Assert.Contains(engine.Options.DynamicTemplates, t => t.Key == key && t.Value == content);
			Assert.Equal(typeof(NoRazorProject), (engine.Handler.Compiler as RazorTemplateCompiler)?.ProjectType);
			Assert.Equal(content, actual);
		}

		[Fact]
		public async Task Ensure_Content_Added_To_DynamicTemplates_When_RazorLightProject_Set_Explicitly_And_Options_Not_Set_Explicitly()
		{
			const string key = "key";
			const string content = "content";

			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseNoProject()
				.Build();

			var actual = await engine.CompileRenderStringAsync(key, content, new object(), new ExpandoObject());

			Assert.NotEmpty(engine.Options.DynamicTemplates);
			Assert.Contains(engine.Options.DynamicTemplates, t => t.Key == key && t.Value == content);
			Assert.Equal(typeof(NoRazorProject), (engine.Handler.Compiler as RazorTemplateCompiler)?.ProjectType);
			Assert.Equal(content, actual);
		}
	}
}
