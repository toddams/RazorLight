using System;
using System.Dynamic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using RazorLight.Compilation;
using RazorLight.Extensions;

namespace RazorLight
{
	public class RazorLightEngine : IDisposable
	{
		private readonly ConfigurationOptions _config;
		private readonly RoslynCompilerService _compilerService;
		private readonly RazorLightCodeGenerator _codeGenerator;

		private readonly PhysicalFileProvider _fileProvider;
		private readonly CompilerCache compilerCache;

		/// <summary>
		/// Initializes new RazorLight engine with a default configuration options
		/// </summary>
		public RazorLightEngine() : this(ConfigurationOptions.Default) { }

		public RazorLightEngine(ConfigurationOptions options)
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			this._config = options;
			_compilerService = new RoslynCompilerService(options);
			_codeGenerator = new RazorLightCodeGenerator(options);

			if (!string.IsNullOrEmpty(options.ViewsFolder))
			{
				_fileProvider = new PhysicalFileProvider(options.ViewsFolder);
				compilerCache = new CompilerCache(_fileProvider);
				CanParseFiles = true;
			}

		}

		/// <summary>
		/// Returns true if ConfigurationOptions's property ViewFolder is set and such folder exists in filesystem
		/// </summary>
		public bool CanParseFiles { get; private set; }

		/// <summary>
		/// Parses given razor template string
		/// </summary>
		/// <typeparam name="T">Type of Model</typeparam>
		/// <param name="content">Razor string</param>
		/// <param name="model"></param>
		/// <returns></returns>
		public string ParseString<T>(string content, T model)
		{
			if (content == null)
			{
				throw new ArgumentNullException(nameof(content));
			}

			if (model == null)
			{
				throw new ArgumentNullException();
			}

			string razorCode = _codeGenerator.GenerateCode(new StringReader(content), model);

			return CompileAndRun<T>(razorCode, model);
		}

		/// <summary>
		/// Parses *.cshtml file with a given relative path and Model. Parsed result is compiled and cached
		/// </summary>
		/// <typeparam name="T">Type of Model</typeparam>
		/// <param name="viewRelativePath">Relative path to the Razor view</param>
		/// <param name="model">Model of the Razor view</param>
		/// <returns></returns>
		public string ParseFile<T>(string viewRelativePath, T model)
		{
			if (!CanParseFiles)
			{
				throw new RazorLightException("Can't parse a file. ViewsFolder must be set in ConfigurationOptions");
			}

			if (viewRelativePath == null)
			{
				throw new ArgumentNullException(nameof(viewRelativePath));
			}

			if (model == null)
			{
				throw new ArgumentNullException(nameof(model));
			}

			string result = compilerCache.GetOrAdd(viewRelativePath, path => OnCompilerCacheMiss(path, model));

			return result;
		}

		private string OnCompilerCacheMiss<T>(string viewRelativePath, T model)
		{
			string fullPath = Path.Combine(_config.ViewsFolder, viewRelativePath);
			if (!File.Exists(fullPath))
			{
				throw new FileNotFoundException("View not found", fullPath);
			}

			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using (var reader = new StreamReader(fileStream))
				{
					string razorCode = _codeGenerator.GenerateCode(reader, model);
					return CompileAndRun(razorCode, model);
				}
			}
			finally
			{
				if (fileStream != null)
				{
					fileStream.Dispose();
				}
			}
		}

		private string CompileAndRun<T>(string razorCode, T model)
		{
			Type compiledType = _compilerService.Compile(razorCode);

			if ((typeof(T).IsAnonymousType()))
			{
				ExpandoObject dynamicModel = model.ToExpando();

				LightRazorPage<dynamic> page = (LightRazorPage<dynamic>)Activator.CreateInstance(compiledType);

				return RunPage(page, dynamicModel);
			}
			else
			{
				LightRazorPage<T> page = (LightRazorPage<T>)Activator.CreateInstance(compiledType);

				return RunPage(page, model);
			}
		}

		private string RunPage<T>(LightRazorPage<T> page, T model)
		{
			using (var stream = new StringWriter())
			{
				page.Model = model;
				page.Output = stream;

				page.ExecuteAsync().Wait();

				return stream.ToString();
			}
		}

		public void Dispose()
		{
			compilerCache?.Dispose();
			_fileProvider?.Dispose();
		}
	}
}