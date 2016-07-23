using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using RazorLight.Host;

namespace RazorLight
{
	public class RazorLightCodeGenerator
	{
		public string GenerateCode(TextReader input, ModelTypeInfo modelTypeInfo)
		{
			LightRazorHost host = new LightRazorHost(modelTypeInfo.TemplateTypeName);
			GeneratorResults generatorResults = new RazorTemplateEngine(host).GenerateCode(input);

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
