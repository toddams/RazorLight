using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;

namespace RazorLight
{
    public class UseEntryAssemblyMetadataResolver : IMetadataResolver
    {
	    public IList<MetadataReference> GetMetadataReferences()
	    {
		    var metadataReferences = new List<MetadataReference>();

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

			return metadataReferences;
	    }
    }
}
