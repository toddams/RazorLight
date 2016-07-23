using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;

namespace RazorLight.Compilation
{
	public class RoslynCompilerService
	{
		private readonly ConfigurationOptions _config;

		private IList<MetadataReference> _compilationReferences;
		private object _compilationReferencesLock = new object();
		private bool _compilationReferencesInitialized;

		public IList<MetadataReference> CompilationReferences
		{
			get
			{
				return LazyInitializer.EnsureInitialized(
					ref _compilationReferences,
					ref _compilationReferencesInitialized,
					ref _compilationReferencesLock,
					GetMetadataReferences);
			}
		}

		public RoslynCompilerService(ConfigurationOptions options)
		{
			if(options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			this._config = options;
		}

		public Type Compile(string content)
		{
			string assemblyName = Path.GetRandomFileName();

			SourceText sourceText = SourceText.From(content, Encoding.UTF8);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
				sourceText,
				path: assemblyName);

			CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

			CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName,
				syntaxTrees: new[] { syntaxTree },
				references: CompilationReferences,
				options: compilationOptions);

			using (var assemblyStream = new MemoryStream())
			{
				using (var pdbStream = new MemoryStream())
				{
					EmitResult result = compilation.Emit(
						assemblyStream,
						pdbStream,
						options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));

					if (!result.Success)
					{
						var errors = result.Diagnostics
							.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error)
							.Select(d => d.GetMessage());

						throw new RazorLightCompilationException(
							"Failed to compile generated razor view. See CompilationErrors for detailed information", errors);
					}
					else
					{
						assemblyStream.Seek(0, SeekOrigin.Begin);
						pdbStream.Seek(0, SeekOrigin.Begin);

						Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream, pdbStream);

						Type type = assembly.GetExportedTypes().FirstOrDefault(a => !a.IsNested);

						return type;
					}
				}
			}
		}

		private List<MetadataReference> GetMetadataReferences()
		{
			var metadataReferences = new List<MetadataReference>();

			if (_config.LoadDependenciesFromEntryAssembly)
			{
				DependencyContext entryAssemblyDependencies = DependencyContext.Load(Assembly.GetEntryAssembly());
				foreach (CompilationLibrary compilationLibrary in entryAssemblyDependencies.CompileLibraries)
				{
					List<string> assemblyPaths = compilationLibrary.ResolveReferencePaths().ToList();
					if (assemblyPaths.Any())
					{
						metadataReferences.Add(MetadataReference.CreateFromFile(assemblyPaths.First()));
					}
				}

				if (!metadataReferences.Any())
				{
					throw new RazorLightException("Can't load metadata reference from the entry assembly. " +
						"Make sure preserveCompilationContext is set to true in compilerOptions section of project.json");
				}
			}

			foreach(var metadata in _config.AdditionalCompilationReferences)
			{
				metadataReferences.Add(metadata);
			}

			return metadataReferences;
		}
	}
}
