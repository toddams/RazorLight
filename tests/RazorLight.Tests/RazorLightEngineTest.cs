using System;
using System.Threading.Tasks;
using Moq;
using RazorLight.Razor;
using RazorLight.Tests.Models;
using RazorLight.Tests.Razor;
using Xunit;

namespace RazorLight.Tests
{
    public class RazorLightEngineTest
    {
		//TODO: add string rendering test

		[Fact]
		public async Task Ensure_Content_Added_To_DynamicTemplates()
		{
			var options = new RazorLightOptions();

			string key = "key";
			string content = "content";
			var project = new TestRazorProject();
			project.Value = new TextSourceRazorProjectItem(key, content);

			var engine = new EngineFactory().Create(project, options);

			await engine.CompileRenderAsync(key, content, new object(), typeof(object));

			Assert.NotEmpty(options.DynamicTemplates);
			Assert.Contains(options.DynamicTemplates, t => t.Key == key && t.Value == content);
		}
	}
}
