using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Parser;
using RazorLight.Host;
using RazorLight.Templating;

namespace RazorLight.Compilation
{
	public class DefaultRazorTemplateCompiler : IRazorTemplateCompiler
	{
		public string CompileTemplate(RazorLightHost host, ITemplateSource templateSource)
		{
			string className = ParserHelpers.SanitizeClassName(templateSource.TemplateKey);
			var templateEngine = new RazorTemplateEngine(host);

			using (var content = templateSource.CreateReader())
			{
				GeneratorResults result = templateEngine.GenerateCode(content, className, host.DefaultNamespace,
					templateSource.FilePath);

				if (!result.Success)
				{
					throw new TemplateParsingException("Failed to parse razor page. See ParserErrors for more details", result.ParserErrors);
				}

				return result.GeneratedCode;
			}
		}
	}
}
