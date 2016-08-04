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
using RazorLight.Internal;

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
			if(metadataResolver == null)
			{
				throw new ArgumentNullException(nameof(metadataResolver));
			}

			this.metadataResolver = metadataResolver;
		}

		public CompilationResult Compile(string content)
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

						return new CompilationResult(errors);
					}
					else
					{
						assemblyStream.Seek(0, SeekOrigin.Begin);
						pdbStream.Seek(0, SeekOrigin.Begin);

						Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream, pdbStream);

						Type type = assembly.GetExportedTypes().FirstOrDefault(a => !a.IsNested);

						return new CompilationResult(type);
					}
				}
			}
		}
	}
}
