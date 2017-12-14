using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using RazorLight.Compilation;
using RazorLight.Generation;
using RazorLight.Razor;
using Xunit;
using DependencyContextCompilationOptions = Microsoft.Extensions.DependencyModel.CompilationOptions;

namespace RazorLight.Tests.Compilation
{
	//TODO: finish
	public class RoslynCompilerServiceTest
    {
		[Fact]
		public void OperatingAssembly_Returns_EntryAssembly()
		{
			var entryAssembly = Assembly.GetEntryAssembly();

			var compiler = new RoslynCompilationService(new DefaultMetadataReferenceManager());

			Assert.NotNull(compiler.OperatingAssembly);
			Assert.Equal(compiler.OperatingAssembly, entryAssembly);
		}

		[Fact]
		public void Constructor_SetsCompilationOptionsFromDependencyContext()
		{
			var compiler = new RoslynCompilationService(new DefaultMetadataReferenceManager());

			// Act & Assert
			var parseOptions = compiler.ParseOptions;
			Assert.Contains("SOME_TEST_DEFINE", parseOptions.PreprocessorSymbolNames);
		}

		[Fact]
		public void EnsureOptions_ConfiguresCompilationOptions()
		{
			// Arrange
			var compiler = new RoslynCompilationService(new DefaultMetadataReferenceManager());

			// Act & Assert
			var compilationOptions = compiler.CSharpCompilationOptions;
			Assert.True(compilationOptions.AllowUnsafe);
			Assert.Equal(ReportDiagnostic.Default, compilationOptions.GeneralDiagnosticOption);
			Assert.Equal(OptimizationLevel.Debug, compilationOptions.OptimizationLevel);
			Assert.Collection(compilationOptions.SpecificDiagnosticOptions.OrderBy(d => d.Key),
				item =>
				{
					Assert.Equal("CS1701", item.Key);
					Assert.Equal(ReportDiagnostic.Suppress, item.Value);
				},
				item =>
				{
					Assert.Equal("CS1702", item.Key);
					Assert.Equal(ReportDiagnostic.Suppress, item.Value);
				},
				item =>
				{
					Assert.Equal("CS1705", item.Key);
					Assert.Equal(ReportDiagnostic.Suppress, item.Value);
				});
		}

		[Fact]
		public void Constructor_ConfiguresLanguageVersion()
		{
			// Arrange
			var dependencyContextOptions = new DependencyContextCompilationOptions(
				new[] { "MyDefine" },
				languageVersion: "7.1",
				platform: null,
				allowUnsafe: true,
				warningsAsErrors: null,
				optimize: null,
				keyFile: null,
				delaySign: null,
				publicSign: null,
				debugType: null,
				emitEntryPoint: null,
				generateXmlDocumentation: null);

			var compiler = new TestCSharpCompiler(new DefaultMetadataReferenceManager(), dependencyContextOptions);

			// Act & Assert
			var compilationOptions = compiler.ParseOptions;
			Assert.Equal(LanguageVersion.CSharp7_1, compilationOptions.LanguageVersion);
		}

		[Fact]
        public void Constructor_ConfiguresAllowUnsafe()
		{
			// Arrange
			var dependencyContextOptions = new DependencyContextCompilationOptions(
				new[] { "MyDefine" },
				languageVersion: null,
				platform: null,
				allowUnsafe: true,
				warningsAsErrors: null,
				optimize: null,
				keyFile: null,
				delaySign: null,
				publicSign: null,
				debugType: null,
				emitEntryPoint: null,
				generateXmlDocumentation: null);

			var compiler = new TestCSharpCompiler(new DefaultMetadataReferenceManager(), dependencyContextOptions);

			// Act & Assert
			var compilationOptions = compiler.CSharpCompilationOptions;
			Assert.True(compilationOptions.AllowUnsafe);
		}

		[Fact]
		public void Constructor_SetsDiagnosticOption()
		{
			// Arrange
			var dependencyContextOptions = new DependencyContextCompilationOptions(
				new[] { "MyDefine" },
				languageVersion: null,
				platform: null,
				allowUnsafe: null,
				warningsAsErrors: true,
				optimize: null,
				keyFile: null,
				delaySign: null,
				publicSign: null,
				debugType: null,
				emitEntryPoint: null,
				generateXmlDocumentation: null);

			var compiler = new TestCSharpCompiler(new DefaultMetadataReferenceManager(), dependencyContextOptions);

			// Act & Assert
			var compilationOptions = compiler.CSharpCompilationOptions;
			Assert.Equal(ReportDiagnostic.Error, compilationOptions.GeneralDiagnosticOption);
		}

		[Fact]
		public void Constructor_SetsOptimizationLevel()
		{
			// Arrange
			var dependencyContextOptions = new DependencyContextCompilationOptions(
				new[] { "MyDefine" },
				languageVersion: null,
				platform: null,
				allowUnsafe: null,
				warningsAsErrors: null,
				optimize: true,
				keyFile: null,
				delaySign: null,
				publicSign: null,
				debugType: null,
				emitEntryPoint: null,
				generateXmlDocumentation: null);

			var compiler = new TestCSharpCompiler(new DefaultMetadataReferenceManager(), dependencyContextOptions);

			// Act & Assert
			var compilationOptions = compiler.CSharpCompilationOptions;
			Assert.Equal(OptimizationLevel.Release, compilationOptions.OptimizationLevel);
		}

		[Fact]
		public void Constructor_SetsDefines()
		{
			// Arrange
			var dependencyContextOptions = new DependencyContextCompilationOptions(
				new[] { "MyDefine" },
				languageVersion: null,
				platform: null,
				allowUnsafe: null,
				warningsAsErrors: null,
				optimize: true,
				keyFile: null,
				delaySign: null,
				publicSign: null,
				debugType: null,
				emitEntryPoint: null,
				generateXmlDocumentation: null);

			var compiler = new TestCSharpCompiler(new DefaultMetadataReferenceManager(), dependencyContextOptions);

			// Act & Assert
			var parseOptions = compiler.ParseOptions;
			Assert.Equal(new[] { "MyDefine", "RELEASE" }, parseOptions.PreprocessorSymbolNames);
		}

		[Fact]
		public void Compile_UsesApplicationsCompilationSettings_ForParsingAndCompilation()
		{
			// Arrange
			var content = "public class Test {}";
			var define = "MY_CUSTOM_DEFINE";
			var dependencyContextOptions = new DependencyContextCompilationOptions(
				new[] { define },
				languageVersion: null,
				platform: null,
				allowUnsafe: null,
				warningsAsErrors: null,
				optimize: true,
				keyFile: null,
				delaySign: null,
				publicSign: null,
				debugType: null,
				emitEntryPoint: null,
				generateXmlDocumentation: null);
			var compiler = new TestCSharpCompiler(new DefaultMetadataReferenceManager() , dependencyContextOptions);
			// Act
			var syntaxTree = compiler.CreateSyntaxTree(SourceText.From(content));
			// Assert
			Assert.Contains(define, syntaxTree.Options.PreprocessorSymbolNames);
		}

		[Fact]
		public async Task Throw_With_CompilationErrors_On_Failed_BuildAsync()
		{
			var compiler = new RoslynCompilationService(new DefaultMetadataReferenceManager());

			var template = new TestGeneratedRazorTemplate("key", "public class Test { error }");

			TemplateCompilationException ex = null;

			try
			{
				await compiler.CompileAsync(template);
			}
			catch (TemplateCompilationException e)
			{
				ex = e;
			}

			
			Assert.NotNull(ex);
			Assert.NotEmpty(ex.CompilationErrors);
			Assert.Equal(1, ex.CompilationErrors.Count);
		}

		[Fact]
		public void Throw_OnNullRazorTemplate_OnCompile()
		{
			var compiler = new RoslynCompilationService(new DefaultMetadataReferenceManager());

			Func<Task> action = () => compiler.CompileAsync(null);

			Assert.ThrowsAsync<ArgumentNullException>(action);
		}

		private class TestGeneratedRazorTemplate : IGeneratedRazorTemplate
		{
			private string generatedCode;
			private string templateKey;

			public TestGeneratedRazorTemplate(string key, string generatedCode)
			{
				this.generatedCode = generatedCode;
				this.templateKey = key;
			}

			public string TemplateKey => templateKey;
			public string GeneratedCode => generatedCode;
            public RazorLightProjectItem ProjectItem
            {
                get
                {
                    return new TextSourceRazorProjectItem(TemplateKey, "");
                }
                set
                {
                    
                }
            }
		}


		private class TestCSharpCompiler : RoslynCompilationService
		{
			private readonly DependencyContextCompilationOptions _options;

			public TestCSharpCompiler(IMetadataReferenceManager referenceManager, DependencyContextCompilationOptions options) : base(referenceManager)
			{
				_options = options;
			}

			protected internal override DependencyContextCompilationOptions GetDependencyContextCompilationOptions()
				=> _options;
		}
	}
}
