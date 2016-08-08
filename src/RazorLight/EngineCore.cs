using System;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Host;
using RazorLight.Templating;

namespace RazorLight
{
	public class EngineCore : IEngineCore
	{
		/// <summary>
		/// Creates <see cref="EngineCore"/> with a default <seealso cref="EngineConfiguration"/>
		/// </summary>
		/// <param name="templateManager">Template manager</param>
		public EngineCore(
			ITemplateManager templateManager) : this(templateManager, EngineConfiguration.Default)
		{
		}

		/// <summary>
		/// Creates <see cref="EngineCore" /> with specified <seealso cref="EngineConfiguration"/>/>
		/// </summary>
		/// <param name="templateManager">Template manager</param>
		/// <param name="configuration">Engine configuration options</param>
		public EngineCore(
			ITemplateManager templateManager,
			IEngineConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			this.TemplateManager = templateManager;
			this.Configuration = configuration;
		}

		public IEngineConfiguration Configuration { get; }
		public ITemplateManager TemplateManager { get; }

		/// <summary>
		/// Generates razor template by parsing given <param name="templateSource" />
		/// </summary>
		/// <param name="templateSource"></param>
		/// <param name="modelTypeInfo"></param>
		/// <returns></returns>
		public string GenerateRazorTemplate(ITemplateSource templateSource, ModelTypeInfo modelTypeInfo)
		{
			var host = new RazorLightHost(null);

			if (modelTypeInfo != null)
			{
				host.DefaultModel = modelTypeInfo.TemplateTypeName;
			}

			return Configuration.RazorTemplateCompiler.CompileTemplate(host, templateSource);
		}

		/// <summary>
		/// Compiles a <see cref="ITemplateSource"/> with a specified <see cref="ModelTypeInfo"/>
		/// </summary>
		/// <param name="templateSource">Template source</param>
		/// <param name="modelTypeInfo">Model type information</param>
		/// <returns>Compiled type in succeded. Compilation errors on fail</returns>
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

		/// <summary>
		/// Compiles a page with a specified <param name="key" />
		/// </summary>
		/// <param name="key"></param>
		/// <returns>Compiled type in succeded. Compilation errors on fail</returns>
		public CompilationResult KeyCompile(string key)
		{
			ITemplateSource source = TemplateManager.Resolve(key);

			return CompileSource(source, null);
		}

		/// <summary>
		/// Activates a type using Activator from <see cref="IEngineConfiguration"/>
		/// </summary>
		/// <param name="compiledType"></param>
		/// <returns></returns>
		public TemplatePage Activate(Type compiledType)
		{
			return (TemplatePage)Configuration.Activator.CreateInstance(compiledType);
		}
	}
}
