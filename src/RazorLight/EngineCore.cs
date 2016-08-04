using System;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Host;
using RazorLight.Templating;

namespace RazorLight
{
	public class EngineCore
	{
		/// <summary>
		/// Creates <see cref="EngineCore"/> with a default <seealso cref="EngineConfiguration"/>
		/// </summary>
		/// <param name="templateManager">Template manager</param>
		/// <param name="compilerCache">Cache where compilation results are stored</param>
		public EngineCore(
			ITemplateManager templateManager,
			ICompilerCache compilerCache) : this(templateManager, compilerCache, EngineConfiguration.Default)
		{
		}

		public EngineCore(
			ITemplateManager templateManager,
			ICompilerCache compilerCache,
			IEngineConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			this.TemplateManager = templateManager;
			this.CompilerCache = compilerCache;
			this.Configuration = configuration;
		}

		public IEngineConfiguration Configuration { get; }
		public ITemplateManager TemplateManager { get; }
		public ICompilerCache CompilerCache { get; }

		public string GenerateRazorTemplate(ITemplateSource templateSource, ModelTypeInfo modelTypeInfo)
		{
			var host = new RazorLightHost(null);

			if (modelTypeInfo != null)
			{
				host.DefaultModel = modelTypeInfo.TemplateTypeName;
			}

			return Configuration.RazorTemplateCompiler.CompileTemplate(host, templateSource);
		}

		public CompilationResult CompileSource(ITemplateSource templateSource, ModelTypeInfo modelTypeInfo)
		{
			if (templateSource == null)
			{
				throw new ArgumentNullException(nameof(templateSource));
			}

			string razorTemplate = GenerateRazorTemplate(templateSource, modelTypeInfo);
			var context = new CompilationContext(razorTemplate, Configuration.Namespaces);

			CompilationResult compilationResult = Configuration.CompilerService.Compile(context);

			return compilationResult;
		}

		public CompilationResult KeyCompile(string key)
		{
			ITemplateSource source = TemplateManager.Resolve(key);

			return CompileSource(source, null);
		}

		public TemplatePage Activate(Type compiledType)
		{
			return (TemplatePage)Configuration.Activator.CreateInstance(compiledType);
		}
	}
}
