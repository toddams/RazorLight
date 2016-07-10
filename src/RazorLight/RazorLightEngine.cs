using System;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Razor;
using RazorLight.Host;
using RazorLight.Compilation;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Text;
using System.Collections.Concurrent;

namespace RazorLight
{
	public class RazorLightEngine
	{
		private readonly ConfigurationOptions _config;
		private RazorTemplateEngine _templateEngine;
		private RoslynCompilerService _compilerService;
		private RazorLightCodeGenerator _codeGenerator;

		private IFileProvider _fileProvider;
		private CompilerCache compilerCache;

		private readonly ConcurrentDictionary<string, string> _normalizedPathLookup =
			new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

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

			if (!string.IsNullOrEmpty(options.ViewsFolder))
			{
				_fileProvider = new PhysicalFileProvider(options.ViewsFolder);
				compilerCache = new CompilerCache(new PhysicalFileProvider(options.ViewsFolder));
				CanParseFiles = true;
			}

		}

		public bool CanParseFiles { get; private set; }

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
			if (!CanParseFiles)
			{
				throw new RazorLightException("Can't parse a file. ViewsFolder must be set in ConfigurationOptions");
			}

			if (viewRelativePath == null)
			{
				throw new ArgumentNullException(viewRelativePath);
			}

			if (model == null)
			{
				throw new ArgumentNullException();
			}

			string result = compilerCache.GetOrAdd(viewRelativePath, path => ProcessRazorPage<T>(_codeGenerator.GenerateCode(path), model));

			return result;
		}

		private string ProcessRazorPage<T>(string razorCode, T model)
		{
			Type compiledType = _compilerService.Compile(razorCode);

			var page = (LightRazorPage<T>)Activator.CreateInstance(compiledType);

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