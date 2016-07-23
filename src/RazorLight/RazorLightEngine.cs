using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Runtime.CompilerServices;
using RazorLight.Compilation;
using Microsoft.Extensions.FileProviders;

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

			string razorCode = _codeGenerator.GenerateCode(new StringReader(content));

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

			string result = compilerCache.GetOrAdd(viewRelativePath, path => CompileAndRun<T>(_codeGenerator.GenerateCode(path), model));

			return result;
		}

		private string CompileAndRun<T>(string razorCode, T model)
		{
			Type compiledType = _compilerService.Compile(razorCode);

			if (IsAnonymousType(typeof(T)))
			{
				ExpandoObject dynamicModel = ToExpando(model);

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

		public static ExpandoObject ToExpando(object anonymousObject)
		{
			IDictionary<string, object> expando = new ExpandoObject();
			foreach (var propertyDescriptor in anonymousObject.GetType().GetTypeInfo().GetProperties())
			{
				var obj = propertyDescriptor.GetValue(anonymousObject);
				expando.Add(propertyDescriptor.Name, obj);
			}

			return (ExpandoObject)expando;
		}

		public static bool IsAnonymousType(Type type)
		{
			bool hasCompilerGeneratedAttribute = type.GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
			bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
			bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

			return isAnonymousType;
		}

		public void Dispose()
		{
			compilerCache?.Dispose();
			_fileProvider?.Dispose();
		}
	}
}