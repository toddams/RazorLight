using System.IO;
using Xunit;

namespace RazorLight.Tests
{
	public class CodeGeneratorTest
    {
		string appRootPath = @"D:\MyProjects\RazorLight\tests\RazorLight.Tests";

		[Fact]
		public void ThrowsIfViewsFolderIsNotSet()
		{
			var config = ConfigurationOptions.Default;

			var codeGenerator = new RazorLightCodeGenerator(config);

			Assert.Throws<RazorLightException>(() => codeGenerator.GenerateCode("Test.cshtml"));
		}

		[Fact]
		public void ThrowsOnInvalidRelativeViewPath()
		{
			var confing = new ConfigurationOptions() { ViewsFolder = Path.Combine(appRootPath, "Views") };

			var codeGenerator = new RazorLightCodeGenerator(confing);

			Assert.Throws<FileNotFoundException>(() => codeGenerator.GenerateCode("NotExistinView.cshtml"));
		}

		[Fact]
		public void SuccessOnValidView()
		{
			var generator = new RazorLightCodeGenerator(new ConfigurationOptions() { ViewsFolder = Path.Combine(appRootPath, "Views") });

			string code = generator.GenerateCode("Test.cshtml");

			Assert.NotNull(code);
		}

		[Fact]
		public void CodeGeneratorGivesCorrectOutput()
		{
			//Arrange
			string view = File.ReadAllText(Path.Combine(appRootPath, "Views/Test.cshtml"));
			string expectedOutput = File.ReadAllText(Path.Combine(appRootPath, "Views/Test.txt"));

			var codeGenerator = new RazorLightCodeGenerator(ConfigurationOptions.Default);

			////Act
			var code = codeGenerator.GenerateCode(new StringReader(view));

			//Assert
			Assert.Equal(code, expectedOutput);
		}
	}
}
