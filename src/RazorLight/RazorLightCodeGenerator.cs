using System.IO;
using System.Text;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using RazorLight.Host;

namespace RazorLight
{
	public class RazorLightCodeGenerator
	{
		private readonly ConfigurationOptions options;

		public RazorLightCodeGenerator(ConfigurationOptions options)
		{
			this.options = options;
		}

		public string GenerateCode(Stream inputStream, ModelTypeInfo modelTypeInfo)
		{
			LightRazorHost host = new LightRazorHost(options);

			GeneratorResults generatorResults = null;
			using (var streamReader = new StreamReader(inputStream))
			{
				generatorResults = new RazorTemplateEngine(host).GenerateCode(streamReader);
			}
			
			if (!generatorResults.Success)
			{
				var builder = new StringBuilder();
				builder.AppendLine("Failed to parse an input:");

				foreach (RazorError error in generatorResults.ParserErrors)
				{
					builder.AppendLine($"{error.Message} (line {error.Location.LineIndex})");
				}

				throw new RazorLightException(builder.ToString());
			}

			return generatorResults.GeneratedCode;
		}
	}
}
