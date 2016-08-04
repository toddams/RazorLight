using System;
using System.Collections.Generic;
using RazorLight.Compilation;

namespace RazorLight
{
    public class EngineConfiguration : IEngineConfiguration
    {
	    public IActivator Activator { get; private set; }
	    public IRazorTemplateCompiler RazorTemplateCompiler { get; private set; } 
	    public ICompilerService CompilerService { get; private set; }

	    public ISet<string> Namespaces { get; set; }

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
	    }

	    public static EngineConfiguration Default => new EngineConfiguration(
		    new DefaultActivator(), 
		    new DefaultRazorTemplateCompiler(), 
		    new RoslynCompilerService(
				new DefaultMetadataResolver()));
    }
}
