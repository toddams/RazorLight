using System;
using System.IO;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Parser;
using Microsoft.Extensions.FileProviders;
using RazorLight.Abstractions;
using RazorLight.Compilation;
using RazorLight.Host;
using RazorLight.Internal;
using RazorLight.Templating;

namespace RazorLight
{
	public class RazorLightEngine
	{
		private readonly ConfigurationOptions config;
		private readonly IFileProvider viewsFileProvider;
		private readonly ICompilerService pageCompiler;
		private readonly ICompilerCache compilerCache;
		private readonly IPageLookup pageLookup;

		public RazorLightEngine() : this(ConfigurationOptions.Default)
		{
		}

		public RazorLightEngine(ConfigurationOptions config)
		{
			if (config == null)
			{
				throw new ArgumentNullException(nameof(config));
			}

			this.config = config;
			this.viewsFileProvider = config.ViewsFileProvider;
			this.compilerCache = new CompilerCache(viewsFileProvider);
			this.pageCompiler = new RoslynCompilerService(config);
			this.pageLookup = new DefaultPageLookup(compilerCache, LookupCompile);
		}

		public ConfigurationOptions Config => config;

		public string ParseString<T>(string content, T model)
		{
			var source = new StringTemplateSource(content);
			var modelTypeInfo = new ModelTypeInfo(typeof(T));

			string razorCode = GenerateRazorTemplate(source, modelTypeInfo);
			Type compiledType = Compile(razorCode).CompiledType;
			TemplatePage page = ActivateType(compiledType);
			page.PageContext = new PageContext() { ModelTypeInfo = modelTypeInfo };

			return RunTemplate(page, model);
		}

		public string ParseFile<T>(string relativeFilePath, T model)
		{
			IFileInfo fileInfo = viewsFileProvider.GetFileInfo(relativeFilePath);
			if (!fileInfo.Exists || fileInfo.IsDirectory)
			{
				throw new FileNotFoundException();
			}

			CompilerCacheResult result = compilerCache.GetOrAdd(relativeFilePath, LookupCompile);

			var context = new PageContext()
			{
				ModelTypeInfo = new ModelTypeInfo(typeof(T)),
				ExecutingFilePath = PathNormalizer.GetNormalizedPath(fileInfo.PhysicalPath)
			};

			TemplatePage templatePage = result.PageFactory();
			templatePage.PageContext = context;

			return RunTemplate(templatePage, model);
		}

		public string GenerateRazorTemplate(ITemplateSource source, ModelTypeInfo modelTypeInfo)
		{
			string className = ParserHelpers.SanitizeClassName(source.TemplateKey);
			RazorLightHost host = CreateHost(modelTypeInfo);
			RazorTemplateEngine templateEngine = new RazorTemplateEngine(host);

			GeneratorResults generatorResults = null;
			using (var content = source.CreateReader())
			{
				if (source.IsPhysicalPage)
				{
					//This overload will pass page's relative path to CodeGeneratorContext param of DecorateCodeGenerator method
					//to grab ViewImports of the template page
					generatorResults = templateEngine.GenerateCode(content, className, host.DefaultNamespace, source.TemplateKey);
				}
				else
				{
					generatorResults = templateEngine.GenerateCode(content);
				}
			}

			if (!generatorResults.Success)
			{
				throw new TemplateParsingException("Failed to parse razor page. See ParserErrors for more details", generatorResults.ParserErrors);
			}

			return generatorResults.GeneratedCode;
		}

		public CompilationResult Compile(string razorCode)
		{
			var result = pageCompiler.Compile(razorCode);
			result.EnsureSuccessful();

			return result;
		}

		public TemplatePage ActivateType(Type compiledType)
		{
			return (TemplatePage)Activator.CreateInstance(compiledType);
		}

		public string RunTemplate(TemplatePage page, object model)
		{
			object pageModel = page.PageContext.ModelTypeInfo.CreateTemplateModel(model);
			page.SetModel(pageModel);
			page.Path = page.PageContext.ExecutingFilePath;

			using (var writer = new StringWriter())
			{
				page.PageContext.Writer = writer;

				using (var renderer = new PageRenderer(page, pageLookup))
				{
					renderer.RenderAsync(page.PageContext).Wait();
					return writer.ToString();
				}
			}
		}

		internal CompilationResult LookupCompile(string path)
		{
			IFileInfo fileInfo = viewsFileProvider.GetFileInfo(path);
			ITemplateSource source = new FileTemplateSource(fileInfo, path);

			string razorCode = GenerateRazorTemplate(source, null);

			return Compile(razorCode);
		}

		private RazorLightHost CreateHost(ModelTypeInfo modelTypeInfo)
		{
			var host = new RazorLightHost(Config.ViewsFileProvider);

			if (modelTypeInfo != null)
			{
				host.DefaultModel = modelTypeInfo.TemplateTypeName;
			}

			return host;
		}
	}
}
