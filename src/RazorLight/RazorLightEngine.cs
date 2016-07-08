using System.IO;
using System.Text;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using RazorLight.Host;
using RazorLight.Compilation;
using System;
using System.Reflection;

namespace RazorLight
{
	public class RazorLightEngine
	{
		private RoslynCompilerService compilerService;

		public RazorLightEngine()
		{
			compilerService = new RoslynCompilerService();
		}

		public string ParseString<T>(string content, T model)
		{
			string code = GenerateCode(new StringReader(content));
			Type compiledType = compilerService.Compile(code);

			string output = ActivatePage(compiledType, model);

			return output;
		}

		public string GenerateCode(TextReader input)
		{
			RazorTemplateEngine engine = new RazorTemplateEngine(new LightRazorHost());

			GeneratorResults generatorResults = null;
			try
			{
				generatorResults = engine.GenerateCode(input);

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
			}
			catch (System.Exception ex) when (!(ex is RazorLightException))
			{
				throw new RazorLightException("Failed to generate a language code. See inner exception", ex);
			}

			return generatorResults.GeneratedCode;
		}

		public string ActivatePage<T>(Type type, T model)
		{
			if (!typeof(LightRazorPage<T>).IsAssignableFrom(type))
			{
				throw new RazorLightException("Invalid page type");
			}

			var page = (LightRazorPage<T>)Activator.CreateInstance(type);
			using (var stream = new StringWriter())
			{
				page.Model = model;
				page.Output = stream;

				page.ExecuteAsync().Wait();

				return stream.ToString();
			}
		}
	}
}