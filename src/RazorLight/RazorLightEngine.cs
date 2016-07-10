using System.IO;
using Microsoft.AspNetCore.Razor;
using RazorLight.Host;
using RazorLight.Compilation;
using System;
using System.Reflection;

namespace RazorLight
{
	public class RazorLightEngine
	{
		private RoslynCompilerService _compilerService;
		private RazorTemplateEngine _templateEngine;
		private RazorLightCodeGenerator _codeGenerator;
		private readonly ConfigurationOptions _config;

		public RazorLightEngine() : this(ConfigurationOptions.Default) { }

		public RazorLightEngine(ConfigurationOptions options)
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			this._config = options;
			_templateEngine = new RazorTemplateEngine(new LightRazorHost());
			_compilerService = new RoslynCompilerService(options);
			_codeGenerator = new RazorLightCodeGenerator(options);
		}

		public string ParseString<T>(string content, T model)
		{
			if(content == null)
			{
				throw new ArgumentNullException(content);
			}

			if(model == null)
			{
				throw new ArgumentNullException();
			}

			string razorCode = _codeGenerator.GenerateCode(new StringReader(content));

			return ProcessRazorPage<T>(razorCode, model);
		}

		public string ParseFile<T>(string viewRelativePath, T model)
		{
			if (viewRelativePath == null)
			{
				throw new ArgumentNullException(viewRelativePath);
			}

			if (model == null)
			{
				throw new ArgumentNullException();
			}

			string razorCode = _codeGenerator.GenerateCode(viewRelativePath);

			return ProcessRazorPage<T>(razorCode, model);
		}

		private string ActivatePage<T>(Type type, T model)
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

		private string ProcessRazorPage<T>(string razorCode, T model)
		{
			Type compiledType = _compilerService.Compile(razorCode);

			return ActivatePage<T>(compiledType, model);
		}
	}
}