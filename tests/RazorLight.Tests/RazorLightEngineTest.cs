using System.Threading.Tasks;
using RazorLight.Razor;
using RazorLight.Tests.Razor;
using Xunit;
using RazorLight.Compilation;
using Moq;
using RazorLight.Caching;
using System;

namespace RazorLight.Tests
{
    public class RazorLightEngineTest
    {
        [Fact]
        public void Throws_On_Empty_EngineOptions()
        {
            Action action = () => new RazorLightEngine(null, new Mock<RazorTemplateCompiler>().Object, new Mock<ITemplateFactoryProvider>().Object, new MemoryCachingProvider());

            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Throws_On_Empty_FactoryProvider()
        {
            Action action = () => new RazorLightEngine(new RazorLightOptions(), new Mock<RazorTemplateCompiler>().Object, null, new MemoryCachingProvider());

            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void Allow_Null_CachingProvider()
        {
            var engine = new RazorLightEngine(new RazorLightOptions(), new Mock<RazorTemplateCompiler>().Object, new Mock<ITemplateFactoryProvider>().Object, cachingProvider: null);

            Assert.NotNull(engine);
            Assert.Null(engine.TemplateCache);
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
