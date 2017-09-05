using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;
using RazorLight.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using DependencyContextCompilationOptions = Microsoft.Extensions.DependencyModel.CompilationOptions;

namespace RazorLight.Compilation
{
    public class RoslynCompiler
    {
        private bool isDevelopment = true;
        private readonly DebugInformationFormat _pdbFormat;
        private List<MetadataReference> metadataReferences = new List<MetadataReference>();

        private bool _optionsInitialized;
        private CSharpParseOptions _parseOptions;
        private CSharpCompilationOptions _compilationOptions;

        public RoslynCompiler()
        {
            _pdbFormat = SymbolsUtility.SupportsFullPdbGeneration() ?
                DebugInformationFormat.Pdb :
                DebugInformationFormat.PortablePdb;

            EmitOptions = new EmitOptions(debugInformationFormat: _pdbFormat);
        }

        public EmitOptions EmitOptions { get; }

        public virtual CSharpCompilationOptions CSharpCompilationOptions
        {
            get
            {
                EnsureOptions();
                return _compilationOptions;
            }
        }

        public virtual CSharpParseOptions ParseOptions
        {
            get
            {
                EnsureOptions();
                return _parseOptions;
            }
        }

        private void EnsureOptions()
        {
            if (!_optionsInitialized)
            {
                var dependencyContextOptions = GetDependencyContextCompilationOptions();
                _parseOptions = GetParseOptions(isDevelopment, dependencyContextOptions);
                _compilationOptions = GetCompilationOptions(isDevelopment, dependencyContextOptions);

                _optionsInitialized = true;
            }
        }

        public Assembly CompileAndEmit(string generatedCode)
        {
            string assemblyName = Path.GetRandomFileName();
            var compilation = CreateCompilation(generatedCode, assemblyName);

            using (var assemblyStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var result = compilation.Emit(
                    assemblyStream,
                    pdbStream,
                    options: EmitOptions);

                if (!result.Success)
                {
                    throw new Exception();
                }

                assemblyStream.Seek(0, SeekOrigin.Begin);
                pdbStream.Seek(0, SeekOrigin.Begin);

                var assembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());

                return assembly;
            }
        }

        protected internal virtual DependencyContextCompilationOptions GetDependencyContextCompilationOptions()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            var dependencyContext = DependencyContext.Load(entryAssembly); //TODO: add option to set entry assembly (or custom)

            //TODO: move to MetadataReferenceManager
            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var references = dependencyContext.CompileLibraries.SelectMany(library => library.ResolveReferencePaths());
            foreach (var reference in references)
            {
                if (libraryPaths.Add(reference))
                {
                    using (var stream = File.OpenRead(reference))
                    {
                        var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                        var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

                        metadataReferences.Add(assemblyMetadata.GetReference(filePath: reference));
                    }
                }
            }

            if (dependencyContext?.CompilationOptions != null)
            {
                return dependencyContext.CompilationOptions;
            }

            return DependencyContextCompilationOptions.Default;
        }

        private CSharpCompilation CreateCompilation(string compilationContent, string assemblyName)
        {
            SourceText sourceText = SourceText.From(compilationContent, Encoding.UTF8);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText,options: ParseOptions)
                                    .WithFilePath(assemblyName);

            var compilation = CSharpCompilation.Create(
                                    assemblyName,
                                    options: CSharpCompilationOptions,
                                    references: metadataReferences)
                                .AddSyntaxTrees(syntaxTree);

            compilation = ExpressionRewriter.Rewrite(compilation);

            //var compilationContext = new RoslynCompilationContext(compilation);
            //_compilationCallback(compilationContext);
            //compilation = compilationContext.Compilation;
            return compilation;
        }

        private static CSharpCompilationOptions GetCompilationOptions(
            bool isDevelopment,
            DependencyContextCompilationOptions dependencyContextOptions)
        {
            var csharpCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            // Disable 1702 until roslyn turns this off by default
            csharpCompilationOptions = csharpCompilationOptions.WithSpecificDiagnosticOptions(
                new Dictionary<string, ReportDiagnostic>
                {
                    {"CS1701", ReportDiagnostic.Suppress}, // Binding redirects
                    {"CS1702", ReportDiagnostic.Suppress},
                    {"CS1705", ReportDiagnostic.Suppress}
                });

            if (dependencyContextOptions.AllowUnsafe.HasValue)
            {
                csharpCompilationOptions = csharpCompilationOptions.WithAllowUnsafe(
                    dependencyContextOptions.AllowUnsafe.Value);
            }

            OptimizationLevel optimizationLevel;
            if (dependencyContextOptions.Optimize.HasValue)
            {
                optimizationLevel = dependencyContextOptions.Optimize.Value ?
                    OptimizationLevel.Release :
                    OptimizationLevel.Debug;
            }
            else
            {
                optimizationLevel = isDevelopment ?
                    OptimizationLevel.Debug :
                    OptimizationLevel.Release;
            }
            csharpCompilationOptions = csharpCompilationOptions.WithOptimizationLevel(optimizationLevel);

            if (dependencyContextOptions.WarningsAsErrors.HasValue)
            {
                var reportDiagnostic = dependencyContextOptions.WarningsAsErrors.Value ?
                    ReportDiagnostic.Error :
                    ReportDiagnostic.Default;
                csharpCompilationOptions = csharpCompilationOptions.WithGeneralDiagnosticOption(reportDiagnostic);
            }

            return csharpCompilationOptions;
        }

        private static CSharpParseOptions GetParseOptions(
            bool isDevelopment,
            DependencyContextCompilationOptions dependencyContextOptions)
        {
            var configurationSymbol = isDevelopment ? "DEBUG" : "RELEASE";
            var defines = dependencyContextOptions.Defines.Concat(new[] { configurationSymbol });

            var parseOptions = new CSharpParseOptions(preprocessorSymbols: defines);

            if (!string.IsNullOrEmpty(dependencyContextOptions.LanguageVersion))
            {
                if (LanguageVersionFacts.TryParse(dependencyContextOptions.LanguageVersion, out var languageVersion))
                {
                    parseOptions = parseOptions.WithLanguageVersion(languageVersion);
                }
                else
                {
                    Debug.Fail($"LanguageVersion {languageVersion} specified in the deps file could not be parsed.");
                }
            }

            return parseOptions;
        }
    }
}
