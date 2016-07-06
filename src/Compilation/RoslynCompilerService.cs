using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;

namespace RazorLight.Compilation
{
	public class RoslynCompilerService
	{
		public Type Compile(string content)
		{
			string assemblyName = Path.GetRandomFileName();

			SourceText sourceText = SourceText.From(content, Encoding.UTF8);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(
				sourceText,
				path: assemblyName);

			List<MetadataReference> metadataReferences = GetMetadataReferences();

			CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

			CSharpCompilation compilation = CSharpCompilation.Create(
				assemblyName,
				syntaxTrees: new[] { syntaxTree },
				references: metadataReferences,
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
						IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
							diagnostic.IsWarningAsError ||
							diagnostic.Severity == DiagnosticSeverity.Error);

						foreach (Diagnostic diagnostic in failures)
						{
							Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
						}

						return null;
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

			DependencyContext entryAssemblyDependencies = DependencyContext.Load(Assembly.GetEntryAssembly());

			foreach (CompilationLibrary compilationLibrary in entryAssemblyDependencies.CompileLibraries)
			{
				IEnumerable<string> assemblyPaths = compilationLibrary.ResolveReferencePaths();
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

			return metadataReferences;
		}
	}
}
