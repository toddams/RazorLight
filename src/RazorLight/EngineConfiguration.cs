using System;
using System.Collections.Generic;
using RazorLight.Compilation;
using RazorLight.Internal;

namespace RazorLight
{
	public class EngineConfiguration : IEngineConfiguration
	{
		/// <summary>
		/// Activator used to create an instance of the compiled templates
		/// </summary>
		public IActivator Activator { get; set; }

		/// <summary>
		/// Class used to compile razor templates into *.cs file
		/// </summary>
		public IRazorTemplateCompiler RazorTemplateCompiler { get; set; }

		/// <summary>
		/// Class used to compile razor templates
		/// </summary>
		public ICompilerService CompilerService { get; set; }

		/// <summary>
		/// Additional namespace to include into template (_ViewImports like)
		/// </summary>
		public ISet<string> Namespaces { get; private set; }

		public EngineConfiguration(
			IActivator activator,
			IRazorTemplateCompiler razorTemplateCompiler,
			ICompilerService compilerService)
		{
			if (activator == null)
			{
				throw new ArgumentNullException(nameof(activator));
			}

			if (razorTemplateCompiler == null)
			{
				throw new ArgumentNullException(nameof(razorTemplateCompiler));
			}

			if (compilerService == null)
			{
				throw new ArgumentNullException(nameof(compilerService));
			}


			this.Activator = activator;
			this.RazorTemplateCompiler = razorTemplateCompiler;
			this.CompilerService = compilerService;

			this.Namespaces = new HashSet<string>();
			this.PreRenderCallbacks = new PreRenderActionList();
		}

		/// <summary>
		/// Creates an <see cref="EngineConfiguration"/> with a default settings
		/// </summary>
		public static EngineConfiguration Default => new EngineConfiguration(
			new DefaultActivator(),
			new DefaultRazorTemplateCompiler(),
			new RoslynCompilerService(
				new UseEntryAssemblyMetadataResolver()));

		public PreRenderActionList PreRenderCallbacks { get; private set; }
	}
}
