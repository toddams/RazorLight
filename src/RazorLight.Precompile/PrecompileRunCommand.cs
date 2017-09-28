using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.CommandLineUtils;
using RazorLight.Compilation;
using RazorLight.Internal;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RazorLight.Precompile
{
    public class PrecompileRunCommand
    {
        private static readonly ParallelOptions ParalellOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 4
        };

        private IRazorLightEngine engine;
        private TemplateFactoryProvider factoryProvider;
        private RoslynCompilationService compiler;

        private CommandLineApplication Application { get; set; }
        private CompilationOptions Options { get; set; }
        private string Extension { get; set; }

        public void Configure(CommandLineApplication app)
        {
            Application = app;
            Options = new CompilationOptions(app);

            app.OnExecute(() => Execute());
        }

        private int Execute()
        {
            if (!ParseArguments())
            {
                return 1;
            }

            engine = new EngineFactory().ForFileSystem(Options.ContentRootOption.Value());
            factoryProvider = (TemplateFactoryProvider)engine.TemplateFactoryProvider;
            compiler = factoryProvider.Compiler;

            var results = GenerateCode();
            bool success = true;

            foreach (var result in results)
            {
                if (result.CSharpDocument.Diagnostics.Count > 0)
                {
                    success = false;
                    foreach (var error in result.CSharpDocument.Diagnostics)
                    {
                        Application.Error.WriteLine($"{result.TemplateFileInfo.FullPath} ({error.Span.LineIndex}): {error.GetMessage()}");
                    }
                }
            }

            if (!success)
            {
                return 1;
            }

            string precompileAssemblyName = $"{Options.ApplicationName}_Precompiled";
            CSharpCompilation compilation = CompileViews(results, precompileAssemblyName);

            string assemblyPath = Path.Combine(Options.OutputPath, precompileAssemblyName + ".dll");
            EmitResult emitResult = EmitAssembly(
                compilation,
                compiler.EmitOptions,
                assemblyPath);

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

        public EmitResult EmitAssembly(
            CSharpCompilation compilation,
            EmitOptions emitOptions,
            string assemblyPath)
        {
            EmitResult emitResult;
            using (var assemblyStream = new MemoryStream())
            {
                using (var pdbStream = new MemoryStream())
                {
                    emitResult = compilation.Emit(
                        assemblyStream,
                        pdbStream,
                        options: emitOptions);

                    if (emitResult.Success)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(assemblyPath));
                        var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
                        assemblyStream.Position = 0;
                        pdbStream.Position = 0;

                        // Avoid writing to disk unless the compilation is successful.
                        using (var assemblyFileStream = File.OpenWrite(assemblyPath))
                        {
                            assemblyStream.CopyTo(assemblyFileStream);
                        }

                        using (var pdbFileStream = File.OpenWrite(pdbPath))
                        {
                            pdbStream.CopyTo(pdbFileStream);
                        }
                    }
                }
            }

            return emitResult;
        }

        private CSharpCompilation CompileViews(ViewCompilationInfo[] results, string assemblyname)
        {
            var compilation = compiler.CreateCompilation(assemblyname);
            var syntaxTrees = new SyntaxTree[results.Length];

            Parallel.For(0, results.Length, ParalellOptions, i =>
            {
                ViewCompilationInfo result = results[i];
                SourceText sourceText = SourceText.From(result.CSharpDocument.GeneratedCode, Encoding.UTF8);

                TemplateFileInfo fileInfo = result.TemplateFileInfo;
                SyntaxTree syntaxTree = compiler.CreateSyntaxTree(sourceText)
                    .WithFilePath(fileInfo.FullPath ?? fileInfo.ViewEnginePath);
                syntaxTrees[i] = syntaxTree;
            });

            compilation = compilation.AddSyntaxTrees(syntaxTrees);
            compilation = ExpressionRewriter.Rewrite(compilation);

            compilation = AssemblyMetadataGenerator.AddAssemblyMetadata(
                compiler,
                compilation,
                Options);

            return compilation;
        }

        private ViewCompilationInfo[] GenerateCode()
        {
            var files = GetFiles();
            var results = new ViewCompilationInfo[files.Count];
            Parallel.For(0, results.Length, ParalellOptions, i =>
            {
                TemplateFileInfo fileInfo = files[i];
                ViewCompilationInfo compilationInfo;
                using (var fileStream = fileInfo.CreateReadStream())
                {
                    var razorTemplate = factoryProvider.SourceGenerator.GenerateCodeAsync(fileInfo.ViewEnginePath).Result;
                    compilationInfo = new ViewCompilationInfo(fileInfo, razorTemplate.CSharpDocument);
                }

                results[i] = compilationInfo;
            });

            return results;
        }

        private List<TemplateFileInfo> GetFiles()
        {
            string contentRoot = Options.ContentRootOption.Value();
            int trimLength = contentRoot.EndsWith("/") ? contentRoot.Length - 1 : contentRoot.Length;
            var files = new List<TemplateFileInfo>();


            foreach(string file in Directory.EnumerateFiles(contentRoot, "*", SearchOption.AllDirectories))
            {
                if(file.EndsWith(Extension))
                {
                    var viewEnginePath = file.Substring(trimLength).Replace('\\', '/');
                    files.Add(new TemplateFileInfo(file, viewEnginePath));
                }
            }

            return files;
        }

        private bool ParseArguments()
        {
            Extension = Options.TemplatesExtension.Value();
            if(string.IsNullOrEmpty(Extension))
            {
                Extension = ".cshtml";
            }

            //if (!Options.ProjectArgument))
            //{
            //    Application.Error.WriteLine("Project path not specified.");
            //    return false;
            //}

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
