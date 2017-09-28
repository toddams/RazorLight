using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RazorLight.Precompile
{
    class PrecompileRunCommand
    {
        private static readonly ParallelOptions ParalellOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 4
        };

        private CommandLineApplication Application { get; set; }

        public void Configure(CommandLineApplication app)
        {
            Application = app;
            //Options = new CompilationOptions(app);

            app.OnExecute(() => Execute());
        }

        private int Execute()
        {
            if (!ParseArguments())
            {
                return 1;
            }

            MvcServiceProvider = new MvcServiceProvider(
                ProjectPath,
                Options.ApplicationNameOption.Value(),
                Options.ContentRootOption.Value(),
                Options.ConfigureCompilationType.Value());

            var results = GenerateCode();
            var success = true;

            foreach (var result in results)
            {
                if (result.CSharpDocument.Diagnostics.Count > 0)
                {
                    success = false;
                    foreach (var error in result.CSharpDocument.Diagnostics)
                    {
                        Application.Error.WriteLine($"{result.ViewFileInfo.FullPath} ({error.Span.LineIndex}): {error.GetMessage()}");
                    }
                }
            }

            if (!success)
            {
                return 1;
            }

            var precompileAssemblyName = $"{Options.ApplicationName}{ViewsFeatureProvider.PrecompiledViewsAssemblySuffix}";
            var compilation = CompileViews(results, precompileAssemblyName);
            var resources = GetResources(results);

            var assemblyPath = Path.Combine(Options.OutputPath, precompileAssemblyName + ".dll");
            var emitResult = EmitAssembly(
                compilation,
                MvcServiceProvider.Compiler.EmitOptions,
                assemblyPath,
                resources);

            if (!emitResult.Success)
            {
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    Application.Error.WriteLine(CSharpDiagnosticFormatter.Instance.Format(diagnostic));
                }

                return 1;
            }

            return 0;
        }

        private bool ParseArguments()
        {
            ProjectPath = Options.ProjectArgument.Value;
            if (string.IsNullOrEmpty(ProjectPath))
            {
                Application.Error.WriteLine("Project path not specified.");
                return false;
            }

            if (!Options.OutputPathOption.HasValue())
            {
                Application.Error.WriteLine($"Option {CompilationOptions.OutputPathTemplate} does not specify a value.");
                return false;
            }

            if (!Options.ApplicationNameOption.HasValue())
            {
                Application.Error.WriteLine($"Option {CompilationOptions.ApplicationNameTemplate} does not specify a value.");
                return false;
            }

            if (!Options.ContentRootOption.HasValue())
            {
                Application.Error.WriteLine($"Option {CompilationOptions.ContentRootTemplate} does not specify a value.");
                return false;
            }

            return true;
        }
    }
}
