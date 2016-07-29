using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Parser;
using Microsoft.Extensions.FileProviders;
using RazorLight.Compilation;
using RazorLight.Host;

namespace RazorLight
{
	public class RazorLightEngine : IDisposable
	{
		private readonly IFileProvider _viewsFileProvider;
		private readonly RoslynCompilerService _pageCompiler;
		private readonly CompilerCache _compilerCache;

		public RazorLightEngine() : this(ConfigurationOptions.Default)
		{
		}

		public RazorLightEngine(ConfigurationOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			this.Config = options;

			this._pageCompiler = new RoslynCompilerService(Config);
			this._viewsFileProvider = options.ViewsFileProvider;
			this._compilerCache = new CompilerCache(_viewsFileProvider);
		}

		public ConfigurationOptions Config { get; private set; }

		/// <summary>
		/// Parses a file with a given relative path and model.
		/// </summary>
		/// <typeparam name="T">Type of the Model to use with a template</typeparam>
		/// <param name="path">View path relative to ViewsFolder path</param>
		/// <param name="model">The model</param>
		/// <returns>The result of executing the template</returns>
		/// <remarks>Compilation result will be cached (path = cache key)</remarks>
		public string ParseFile<T>(string path, T model)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

			return _compilerCache.GetOrAdd(path, s => OnCompilerCacheMiss(path, model));
		}

		/// <summary>
		/// Parses a string with a given model
		/// </summary>
		/// <typeparam name="T">Type of the Model to use with a template</typeparam>
		/// <param name="content">String that represents a razor template</param>
		/// <param name="model">The model</param>
		/// <returns>The result of executing the template</returns>
		/// <remarks>Compilation result will NOT be cached</remarks>
		public string ParseString<T>(string content, T model)
		{
			if (string.IsNullOrEmpty(content))
			{
				throw new ArgumentNullException(nameof(content));
			}

			var pageContext = new PageContext()
			{
				IsPhysicalPage = false,
				ModelTypeInfo = new ModelTypeInfo(typeof(T))
			};

			string razorTemplate = null;
			using (var reader = new StringReader(content))
			{
				razorTemplate = GenerateRazorTemplate(reader, pageContext);
			}

			TemplatePage page = Compile<T>(razorTemplate, pageContext);

			return RunTemplate(page, pageContext, model);
		}

		private string OnCompilerCacheMiss<T>(string path, T model)
		{
			IFileInfo templateFileInfo = _viewsFileProvider.GetFileInfo(path);

			if (templateFileInfo.IsDirectory)
			{
				throw new ArgumentException("Invalid file path");
			}

			if (!templateFileInfo.Exists)
			{
				throw new FileNotFoundException("File not found", templateFileInfo.PhysicalPath);
			}

			var pageContext = new PageContext()
			{
				IsPhysicalPage = true,
				ModelTypeInfo = new ModelTypeInfo(typeof(T)),
				PageKey = path,
				ExecutingFilePath = templateFileInfo.PhysicalPath
			};

			string razorTemplate = null;
			using (var stream = templateFileInfo.CreateReadStream())
			using (var reader = new StreamReader(stream))
			{
				razorTemplate = GenerateRazorTemplate(reader, pageContext);
			}

			TemplatePage page = Compile<T>(razorTemplate, pageContext);

			return RunTemplate(page, pageContext, model);
		}

		private string GenerateRazorTemplate(TextReader content, PageContext pageContext)
		{
			string path = pageContext.IsPhysicalPage ? pageContext.PageKey : Path.GetFileName(Path.GetRandomFileName());
			string className = ParserHelpers.SanitizeClassName(path);
			var host = CreateHost(pageContext.ModelTypeInfo.TemplateTypeName);
			var templateEngine = new RazorTemplateEngine(host);

			GeneratorResults generatorResults = null;

			if (pageContext.IsPhysicalPage)
			{
				//This overload will pass page's relative path to CodeGeneratorContext param of DecorateCodeGenerator method
				//to grab ViewImports of the template page
				generatorResults = templateEngine.GenerateCode(content, className, host.DefaultNamespace, pageContext.PageKey);
			}
			else
			{
				generatorResults = templateEngine.GenerateCode(content);
			}

			if (!generatorResults.Success)
			{
				var builder = new StringBuilder();
				builder.AppendLine("Failed to parse razor page:");

				foreach (RazorError error in generatorResults.ParserErrors)
				{
					builder.AppendLine($"{error.Message} (line {error.Location.LineIndex})");
				}

				throw new RazorLightException(builder.ToString());
			}

			return generatorResults.GeneratedCode;
		}

		private TemplatePage Compile<T>(string razorCode, PageContext context)
		{
			Type compiledType = _pageCompiler.Compile(razorCode);

			TemplatePage templatepage = null;

			if (context.ModelTypeInfo.IsStrongType)
			{
				templatepage = (TemplatePage<T>) Activator.CreateInstance(compiledType);
			}
			else
			{
				templatepage = (TemplatePage<dynamic>) Activator.CreateInstance(compiledType);
			}

			return templatepage;
		}

		private string RunTemplate(TemplatePage page, PageContext context, object model)
		{
			object pageModel = context.ModelTypeInfo.CreateTemplateModel(model);
			page.SetModel(pageModel);
			page.Path = context.ExecutingFilePath;

			using (var writer = new StringWriter())
			{
				context.Writer = writer;

				using (var renderer = new PageRenderer(page))
				{
					renderer.RenderAsync(context).Wait();
					return writer.ToString();
				}
			}
		}

		private RazorLightHost CreateHost(string typeName)
		{
			return new RazorLightHost(Config.ViewsFileProvider) { DefaultModel = typeName};
		}

		public void Dispose()
		{
			if (_compilerCache != null)
			{
				_compilerCache.Dispose();
			}
		}
	}
}
