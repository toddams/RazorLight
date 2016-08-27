using System;
using System.Collections.Generic;
using RazorLight.Compilation;
using RazorLight.Host;
using RazorLight.Templating;
using Xunit;

namespace RazorLight.Tests
{
	public class RoslynCompilerTest
	{
		private CompilationResult? testCompilation;
		private readonly IMetadataResolver metadataResolver = new UseEntryAssemblyMetadataResolver();

		private string GetRazorTemplate()
		{
			var razorCompiler = new DefaultRazorTemplateCompiler();
			var source = new LoadedTemplateSource("Hello @Model.Title");

			return razorCompiler.CompileTemplate(new RazorLightHost(null), source);
		}

		private CompilationResult TestCompilation(ISet<string> namespaces = null)
		{
			if (testCompilation == null)
			{
				var compiler = new RoslynCompilerService(metadataResolver);
				var context = new CompilationContext(GetRazorTemplate(), namespaces ?? new HashSet<string>());

				testCompilation = compiler.Compile(context);
			}

			return testCompilation.Value;
		}

		[Fact]
		public void Compiler_No_CompilationFailures()
		{
			CompilationResult compilerResult = TestCompilation();

			Assert.Null(compilerResult.CompilationFailures);
		}

		[Fact]
		public void Compiler_Template_IsAssignable_From_TemplatePage()
		{
			var expectedType = typeof(TemplatePage);

			CompilationResult compilerResult = TestCompilation();
			var actualType = new DefaultActivator().CreateInstance(compilerResult.CompiledType);

			Assert.IsAssignableFrom(expectedType, actualType);
		}

		[Fact]
		public void Compiler_Throw_On_Corrupted_Template()
		{
			var content = "This is a corrupted. @UnknownType should throw";
			var source = new LoadedTemplateSource(content);

			var razorTemplate = new DefaultRazorTemplateCompiler().CompileTemplate(new RazorLightHost(null), source);
			var context = new CompilationContext(razorTemplate, new HashSet<string>());

			CompilationResult result = new RoslynCompilerService(metadataResolver).Compile(context);
			var action = new Action(() => result.EnsureSuccessful());

			Assert.NotNull(result.CompilationFailures);
			Assert.Throws(typeof(TemplateCompilationException), action);
		}
	}
}
