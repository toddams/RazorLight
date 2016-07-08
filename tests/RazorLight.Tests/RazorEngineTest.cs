using System.IO;
using Xunit;
using RazorLight.Compilation;

namespace RazorLight.Tests
{
	public class RazorEngineTest
    {
		string appRootPath = @"D:\MyProjects\RazorLight\tests\RazorLight.Tests";

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

			var compiler = new RoslynCompilerService();
			var engine = new RazorLightEngine();

			string code = engine.GenerateCode(new StringReader(view));

			Assert.Throws<RazorLightCompilationException>(() => compiler.Compile(code));
		}
    }
}
