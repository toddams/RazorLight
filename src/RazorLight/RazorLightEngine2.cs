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
	public class RazorLightEngine2
	{
		private readonly IFileProvider _viewsFileProvider;
		private readonly PageRenderer _pageRenderer;
		private readonly RoslynCompilerService _pageCompiler;
		private readonly CompilerCache _compilerCache;

		public RazorLightEngine2(ConfigurationOptions options)
		{
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			this.Config = options;

			this._pageRenderer = new PageRenderer();
			this._pageCompiler = new RoslynCompilerService(Config);
			this._compilerCache = new CompilerCache(_viewsFileProvider);
			this._viewsFileProvider = options.ViewsFileProvider;
		}

		public ConfigurationOptions Config { get; private set; }

		public string ParseFile<T>(string path, T model)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

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
				ModelTypeInfo = new ModelTypeInfo<T>(model),
				PageKey = path,
				ExecutingFilePath = templateFileInfo.PhysicalPath
			};

			string razorTemplate = null;
			using (var stream = templateFileInfo.CreateReadStream())
			{
				razorTemplate = GenerateRazorTemplate(stream, pageContext);
			}
		}

		public string ParseString<T>(string content, T model)
		{
			if (string.IsNullOrEmpty(content))
			{
				throw new ArgumentNullException(nameof(content));
			}

			var pageContext = new PageContext()
			{
				IsPhysicalPage = false,
				ModelTypeInfo = new ModelTypeInfo<T>(model)
			};

			string razorTemplate = null;
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
			{
				razorTemplate = GenerateRazorTemplate(stream, pageContext);
			}

			return null;
		}

		public string Go<T>(string path, T model)
		{
			IFileInfo info = _viewsFileProvider.GetFileInfo(path);

			var modelTypeInfo = new ModelTypeInfo<T>(model);

			using (var stream = info.CreateReadStream())
			{
				RazorLightHost host = CreateHost(modelTypeInfo.TemplateTypeName);
				string razorcode = host.GenerateCode(path, stream).GeneratedCode;
				Type type = _pageCompiler.Compile(razorcode);

				var page = (TemplatePage<T>) Activator.CreateInstance(type);
				page.Model = model;
				page.Path = info.PhysicalPath;
				page.Layout = "_Layout.cshtml";

				using (var writer = new StringWriter())
				{
					var renderer = new PageRenderer();
					renderer.RazorPage = page;
					renderer.RenderAsync(new PageContext()
					{
						IsPhysicalPage = true,
						ExecutingFilePath = info.PhysicalPath,
						Writer = writer
					}).Wait();

					return writer.ToString();
				}
			}
		}

		private string GenerateRazorTemplate(Stream content, PageContext pageContext)
		{
			string path = pageContext.IsPhysicalPage ? pageContext.PageKey : Path.GetFileName(Path.GetRandomFileName());
			string className = ParserHelpers.SanitizeClassName(path);
			var host = CreateHost(pageContext.ModelTypeInfo.TemplateTypeName);
			var templateEngine = new RazorTemplateEngine(host);

			GeneratorResults generatorResults = null;

			if (pageContext.IsPhysicalPage)
			{
				generatorResults = templateEngine.GenerateCode(content, className, host.DefaultNamespace, pageContext.PageKey);
			}
			else
			{
				using (var reader = new StreamReader(content))
				{
					generatorResults = templateEngine.GenerateCode(reader);
				}
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

		public RazorLightHost CreateHost(string typeName)
		{
			return new RazorLightHost(Config.ViewsFileProvider) { DefaultModel = typeName};
		}
	}
}
