using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.IO;
using System.Reflection.PortableExecutable;

namespace RazorLight.Compilation
{
    public class DefaultMetadataReferenceManager : IMetadataReferenceManager
    {
        public IReadOnlyList<MetadataReference> Resolve(Assembly assembly)
        {
            var dependencyContext = DependencyContext.Load(assembly); //TODO: add option to set entry assembly (or custom)

            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var references = dependencyContext.CompileLibraries.SelectMany(library => library.ResolveReferencePaths());

            var metadataRerefences = new List<MetadataReference>();

            foreach (var reference in references)
            {
                if (libraryPaths.Add(reference))
                {
                    using (var stream = File.OpenRead(reference))
                    {
                        var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                        var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

                        metadataRerefences.Add(assemblyMetadata.GetReference(filePath: reference));
                    }
                }
            }

            return metadataRerefences;
        }
    }
}
