using System;
using RazorLight.Compilation;
using RazorLight.Internal;

namespace RazorLight
{
    public class EngineConfiguration : IEngineConfiguration
    {
	    public IActivator Activator { get; private set; }
	    public IRazorTemplateCompiler RazorTemplateCompiler { get; private set; } 
	    public ICompilerService CompilerService { get; private set; }

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
	    }

	    public static EngineConfiguration Default => new EngineConfiguration(
		    new DefaultActivator(), 
		    new DefaultRazorTemplateCompiler(), 
		    new RoslynCompilerService(
				new DefaultMetadataResolver()));
    }
}
