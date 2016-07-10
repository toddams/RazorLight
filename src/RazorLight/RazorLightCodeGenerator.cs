using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using RazorLight.Host;

namespace RazorLight
{
	public class RazorLightCodeGenerator
    {
		private RazorTemplateEngine _templateEngine;
		private readonly ConfigurationOptions _config;

		public RazorLightCodeGenerator(ConfigurationOptions options)
		{
			if(options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_templateEngine = new RazorTemplateEngine(new LightRazorHost());
		}

		public string GenerateCode(string viewRelativePath)
		{
			if (string.IsNullOrEmpty(_config.ViewsFolder))
			{
				throw new RazorLightException("Can't parse a file. ViewsFolder must be set in ConfigurationOptions");
			}

			if (string.IsNullOrEmpty(viewRelativePath))
			{
				throw new ArgumentNullException(nameof(viewRelativePath));
			}

			string fullPath = Path.Combine(_config.ViewsFolder, viewRelativePath);
			if (!File.Exists(fullPath))
			{
				throw new FileNotFoundException("View not found", fullPath);
			}

			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using(var reader = new StreamReader(fileStream))
				{
					return GenerateCode(reader);
				}
			}
			finally
			{
				if(fileStream != null)
				{
					fileStream.Dispose();
				}
			}
		}

		public string GenerateCode(TextReader input)
		{
			GeneratorResults generatorResults = null;
			try
			{
				generatorResults = _templateEngine.GenerateCode(input);

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
			catch (Exception ex) when (!(ex is RazorLightException))
			{
				throw new RazorLightException("Failed to generate a language code. See inner exception", ex);
			}

			return generatorResults.GeneratedCode;
		}
	}
}
