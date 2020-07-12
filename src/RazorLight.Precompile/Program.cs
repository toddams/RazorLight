using System;

namespace RazorLight.Precompile
{
	class Program
	{
		private readonly static Type ProgramType = typeof(Program);

		static int Main(string[] args)
		{
			var app = new PrecompilationApplication(ProgramType);
			new PrecompileRunCommand().Configure(app);
			return app.Execute(args);
		}
	}
}
