using System.IO;
using RazorLight.Compilation;
using Xunit;

namespace RazorLight.Tests
{
	public class RoslynCompilerServiceTest
	{
		private string testContent = @"
							@model RazorLight.Tests.TestViewModel
							<div>Test @Model.Title</div>
						";

		private PageContext testContext = new PageContext()
		{
			IsPhysicalPage = false,
			ModelTypeInfo = new ModelTypeInfo(typeof(TestViewModel))
		};

		private TemplatePage TestCompileString<T>(RazorLightEngine engine, string content, T model, PageContext context)
		{
			var type = engine.Compile(content);
			return engine.ActivateType(type.CompiledType);
		}

		[Fact]
		public void CompilerThrowsWhenGeneratedCodeHasErrors()
		{
			string view = @"
							@model RazorLight.Tests.TestViewModel
							<div>Test @Model123.Title</div>
						";

			var context = new PageContext()
			{
				IsPhysicalPage = false,
				ModelTypeInfo = new ModelTypeInfo(typeof(TestViewModel))
			};

			var compiler = new RoslynCompilerService(ConfigurationOptions.Default);

			string code = new RazorLightEngine().GenerateRazorTemplate(new StringReader(view), context);

			Assert.Throws<TemplateCompilationException>(() => compiler.Compile(code));
		}

		[Fact]
		public void CompilationServiceHasNoMetadataRefs()
		{
			var options = ConfigurationOptions.Default;
			options.LoadDependenciesFromEntryAssembly = false;
			var compiler = new RoslynCompilerService(options);

			Assert.NotNull(compiler.CompilationReferences);
			Assert.Equal(compiler.CompilationReferences.Count, 0);
		}

		[Fact]
		public void Compiled_Produces_NonEmpty_Type()
		{
			var compiler = new RoslynCompilerService(ConfigurationOptions.Default);
			var engine = new RazorLightEngine();

			var code = engine.GenerateRazorTemplate(new StringReader(testContent), testContext);
			var type = compiler.Compile(code);

			Assert.NotNull(type);
		}

		[Fact]
		public void Activated_Template_Is_Assignable_To_GenericType()
		{
			var engine = new RazorLightEngine();

			var code = engine.GenerateRazorTemplate(new StringReader(testContent), testContext);
			var activatedPage = TestCompileString(engine, code, new TestViewModel(), testContext);

			Assert.NotNull(activatedPage);
		}

		[Fact]
		public void Activated_Template_Has_Dynamic_Model_On_AnonymousType_Model()
		{
			var engine = new RazorLightEngine();

			string content = "Hello @Model.Title";
			var model = new
			{
				Title = "Johny"
			};
			var context = new PageContext() {IsPhysicalPage = false, ModelTypeInfo = new ModelTypeInfo(model.GetType())};

			string code = engine.GenerateRazorTemplate(new StringReader(content), context);
			var template = TestCompileString(engine, code, model, context) as TemplatePage<dynamic>;

			Assert.NotNull(template);
		}

	}
}
