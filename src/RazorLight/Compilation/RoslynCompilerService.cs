using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if NETSTANDARD1_6
using System.Runtime.Loader;
#endif
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace RazorLight.Compilation
{
	public class RoslynCompilerService : ICompilerService
	{
		private readonly IMetadataResolver metadataResolver;
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
					metadataResolver.GetMetadataReferences);
			}
		}

		public RoslynCompilerService(IMetadataResolver metadataResolver)
		{
			if (metadataResolver == null)
			{
				throw new ArgumentNullException(nameof(metadataResolver));
			}

			this.metadataResolver = metadataResolver;
		}

		public CompilationResult Compile(CompilationContext context)
		{
			string assemblyName = Path.GetRandomFileName();

			SourceText sourceText = SourceText.From(context.Content, Encoding.UTF8);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
				sourceText,
				path: assemblyName);

			CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
				.WithUsings(context.IncludeNamespaces);

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
						List<Diagnostic> errorsDiagnostics = result.Diagnostics
							.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error)
							.ToList();

						var errorMessages = new List<string>();

						foreach(Diagnostic diagnostic in errorsDiagnostics)
						{
							FileLinePositionSpan lineSpan = diagnostic.Location.SourceTree.GetMappedLineSpan(diagnostic.Location.SourceSpan);
							string errorMessage = diagnostic.GetMessage();

							errorMessages.Add($"({lineSpan.StartLinePosition.Line}:{lineSpan.StartLinePosition.Character}) {errorMessage}");
						}

						return new CompilationResult(errorMessages);
					}
					else
					{
						assemblyStream.Seek(0, SeekOrigin.Begin);
						pdbStream.Seek(0, SeekOrigin.Begin);

#if NETSTANDARD1_6
						Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream, pdbStream);
#elif NET451
						Assembly assembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());
#endif

						Type type = assembly.GetExportedTypes().FirstOrDefault(a => !a.IsNested);
						return new CompilationResult(type);
					}
				}
			}
		}
	}
}
