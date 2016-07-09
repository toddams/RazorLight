using System.IO;
using Xunit;
using RazorLight.Compilation;

namespace RazorLight.Tests
{
	public class RazorEngineTest
    {
		string appRootPath = @"D:\MyProjects\RazorLight\tests\RazorLight.Tests";

		[Fact]
		public void ConfigureOptionsHasDefaultValues()
		{
			var options = new ConfigurationOptions();

			Assert.NotNull(options.AdditionalCompilationReferences);
			Assert.Equal(options.AdditionalCompilationReferences.Count, 0);
			Assert.NotNull(options.LoadDependenciesFromEntryAssembly);
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
		public void ActivationThrowsWhenPageIsNotRazorLightDescendant()
		{
			var engine = new RazorLightEngine();

			Assert.Throws<RazorLightException>(() => engine.ActivatePage<TestViewModel>(typeof(object), new TestViewModel()));
		}

		[Fact]
		public void CodeGeneratorGivesCorrectOutput()
		{
			//Arrange
			string view = File.ReadAllText(Path.Combine(appRootPath, "Views/Test.cshtml"));
			string expectedOutput = File.ReadAllText(Path.Combine(appRootPath, "Views/Test.txt"));
			var engine = new RazorLightEngine();

			//Act
			var code = engine.GenerateCode(new StringReader(view));

			//Assert
			Assert.Equal(code, expectedOutput);
		}

		[Fact]
		public void CompilerThrowsWhenGeneratedCodeHasErrors()
		{
			string view = @"
							@model RazorLight.Tests.TestViewModel
							<div>Test @Model123.Title</div>
						";

			var compiler = new RoslynCompilerService(ConfigurationOptions.Default);
			var engine = new RazorLightEngine();

			string code = engine.GenerateCode(new StringReader(view));

			Assert.Throws<RazorLightCompilationException>(() => compiler.Compile(code));
		}
    }
}
