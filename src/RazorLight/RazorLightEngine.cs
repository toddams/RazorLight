using System;
using System.Dynamic;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using RazorLight.Compilation;
using RazorLight.Extensions;

namespace RazorLight
{
	public class RazorLightEngine : IDisposable
	{
		private readonly ConfigurationOptions _config;
		private readonly RoslynCompilerService _compilerService;
		private readonly RazorLightCodeGenerator _codeGenerator; //TODO: Remove

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

			_config = options;
			_codeGenerator = new RazorLightCodeGenerator(options);
			_compilerService = new RoslynCompilerService(options);

			if (options.ViewsFileProvider != null)
			{
				compilerCache = new CompilerCache(_config.ViewsFileProvider);
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

			var modelTypeInfo = new ModelTypeInfo<T>(model);

			string razorCode = null;
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				razorCode = _codeGenerator.GenerateCode(stream, modelTypeInfo);
			}

			return CompileAndRun<T>(razorCode, modelTypeInfo);
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

			if (!_config.ViewsFileProvider.GetFileInfo(viewRelativePath).Exists)
			{
				throw new FileNotFoundException("View not found", viewRelativePath);
			}

			string result = compilerCache.GetOrAdd(viewRelativePath, path => OnCompilerCacheMiss(path, model));

			return result;
		}

		private string OnCompilerCacheMiss<T>(string viewRelativePath, T model)
		{
			IFileInfo fileInfo = _config.ViewsFileProvider.GetFileInfo(viewRelativePath);

			if (!fileInfo.Exists)
			{
				throw new FileNotFoundException("View not found", viewRelativePath);
			}

			using (Stream stream = fileInfo.CreateReadStream())
			{
				ModelTypeInfo<T> modelTypeInfo = new ModelTypeInfo<T>(model);

				string razorCode = _codeGenerator.GenerateCode(stream, modelTypeInfo);
				return CompileAndRun(razorCode, modelTypeInfo);
			}
		}

		private string CompileAndRun<T>(string razorCode, ModelTypeInfo<T> modelTypeInfo)
		{
			Type compiledType = _compilerService.Compile(razorCode);

			if (modelTypeInfo.IsAnonymousType)
			{
				ExpandoObject dynamicModel = modelTypeInfo.Value.ToExpando();
				TemplatePage<dynamic> page = (TemplatePage<dynamic>)Activator.CreateInstance(compiledType);

				return RunPage(page, dynamicModel);
			}
			else
			{
				TemplatePage<T> page = (TemplatePage<T>)Activator.CreateInstance(compiledType);

				return RunPage<T>(page, modelTypeInfo.Value);
			}
		}

		private string RunPage<T>(TemplatePage<T> page, T model)
		{
			using (var stream = new StringWriter())
			{
				page.Model = model;
				//page.Output = stream;

				page.ExecuteAsync().Wait();

				return stream.ToString();
			}
		}

		public void Dispose()
		{
			compilerCache?.Dispose();

			//Dispose inner filewathcer in case of using PhysicalFileProvider
			(_config.ViewsFileProvider as PhysicalFileProvider)?.Dispose(); 
		}
	}
}