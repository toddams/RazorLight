using RazorLight.Compilation;
using RazorLight.Host;
using RazorLight.Templating;
using Xunit;

namespace RazorLight.Tests
{
	public class DefaultRazorTemplateCompilerTest
	{
		[Fact]
		public void Compilation_Result_NotNull()
		{
			var compiler = new DefaultRazorTemplateCompiler();
			var source = new LoadedTemplateSource("Hello @Model.Title");

			string result = compiler.CompileTemplate(new RazorLightHost(null), source);

			Assert.NotNull(result);
		}
	}
}
