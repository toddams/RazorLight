using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using RazorLight.Compilation;
using System.Reflection;

namespace RazorLight.Precompile
{
	internal static class AssemblyMetadataGenerator
	{
		public static CSharpCompilation AddAssemblyMetadata(
			RoslynCompilationService compiler,
			CSharpCompilation compilation,
			CompilationOptions compilationOptions)
		{
			var applicationAssemblyName = Assembly.Load(new AssemblyName(compilationOptions.ApplicationName)).GetName();
			var assemblyVersionContent = $"[assembly:{typeof(AssemblyVersionAttribute).FullName}(\"{applicationAssemblyName.Version}\")]";
			var syntaxTree = compiler.CreateSyntaxTree(SourceText.From(assemblyVersionContent));
			return compilation.AddSyntaxTrees(syntaxTree);
		}
	}
}
