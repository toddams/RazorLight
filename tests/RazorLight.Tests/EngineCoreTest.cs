using Xunit;
using System;
using RazorLight;
using RazorLight.Templating;
using RazorLight.Templating.Embedded;
using RazorLight.Compilation;
using Moq;

namespace RazorLight.Tests
{
	public class EngineCoreTest
    {
		private IEngineCore testCore => new EngineCore(new EmbeddedResourceTemplateManager(typeof(EngineCoreTest)));

		[Fact]
		public void Throws_On_Null_Configuration()
		{
			Assert.Throws<ArgumentNullException>(() => new EngineCore(new EmbeddedResourceTemplateManager(typeof(EngineCoreTest)), null));
		}

		[Fact]
		public void Throws_On_Null_TemplateSource()
		{
			Assert.Throws<ArgumentNullException>(() => testCore.CompileSource(null, new ModelTypeInfo(typeof(int))));
		}

		[Fact]
		public void Ensure_CompileSource_Returns_Correct_CompilationResult()
		{
			var source = new LoadedTemplateSource("Hello, @Model.Title");
			var model = new { Title = "John" };
			var modelInfo = new ModelTypeInfo(model.GetType());

			CompilationResult result = testCore.CompileSource(source, modelInfo);

			Assert.NotNull(result);
			Assert.Null(result.CompilationFailures);
			Assert.NotNull(result.CompiledType);
		}

		[Fact]
		public void Ensure_KeyCompile_Resolved_And_Compiles_Template()
		{
			var templateManager = new Mock<ITemplateManager>();
			templateManager.Setup(t => t.Resolve(It.IsAny<string>())).Returns(new LoadedTemplateSource("Hello @Model.Title"));

			IEngineCore core = new EngineCore(templateManager.Object);
			CompilationResult result = core.KeyCompile("whatever");

			Assert.NotNull(result);
			Assert.Null(result.CompilationFailures);
			Assert.NotNull(result.CompiledType);
		}

		[Fact]
		public void Ensure_CompiledType_Is_TypeOf_TemplatePage()
		{
			var source = new LoadedTemplateSource("Hello, @Model.Title");
			var model = new { Title = "John" };
			var modelInfo = new ModelTypeInfo(model.GetType());

			Type type = testCore.CompileSource(source, modelInfo).CompiledType;
			var page = Activator.CreateInstance(type);

			Assert.IsAssignableFrom<TemplatePage>(page);
		}
    }
}
