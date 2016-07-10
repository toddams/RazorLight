using System.IO;
using RazorLight.Compilation;
using Xunit;

namespace RazorLight.Tests
{
	public class RoslynCompilerServiceTest
    {
		[Fact]
		public void CompilerThrowsWhenGeneratedCodeHasErrors()
		{
			string view = @"
							@model RazorLight.Tests.TestViewModel
							<div>Test @Model123.Title</div>
						";

			var compiler = new RoslynCompilerService(ConfigurationOptions.Default);
			var codeGenerator = new RazorLightCodeGenerator(ConfigurationOptions.Default);

			string code = codeGenerator.GenerateCode(new StringReader(view));

			Assert.Throws<RazorLightCompilationException>(() => compiler.Compile(code));
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
	}
}
