using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.Linq;
using System.IO;
using System.Reflection.PortableExecutable;
using Microsoft.Extensions.Options;

namespace RazorLight.Compilation
{
	public class DefaultMetadataReferenceManager : IMetadataReferenceManager
	{
		private readonly IAssemblyPathFormatter _pathFormatter = new DefaultAssemblyPathFormatter();
		public HashSet<MetadataReference> AdditionalMetadataReferences { get; }
		public HashSet<string> ExcludedAssemblies { get; }

		public DefaultMetadataReferenceManager()
		{
			AdditionalMetadataReferences = new HashSet<MetadataReference>();
			ExcludedAssemblies = new HashSet<string>();
		}

		public DefaultMetadataReferenceManager(IOptions<RazorLightOptions> options, IAssemblyPathFormatter pathFormatter) : this(options.Value.AdditionalMetadataReferences, options.Value.ExcludedAssemblies)
		{
			_pathFormatter = pathFormatter;
		}

		public DefaultMetadataReferenceManager(HashSet<MetadataReference> metadataReferences)
		{
			AdditionalMetadataReferences = metadataReferences ?? throw new ArgumentNullException(nameof(metadataReferences));
			ExcludedAssemblies = new HashSet<string>();
		}

		public DefaultMetadataReferenceManager(HashSet<MetadataReference> metadataReferences, HashSet<string> excludedAssemblies)
		{
			AdditionalMetadataReferences = metadataReferences ?? throw new ArgumentNullException(nameof(metadataReferences));
			ExcludedAssemblies = excludedAssemblies ?? throw new ArgumentNullException(nameof(excludedAssemblies));
		}

		public IReadOnlyList<MetadataReference> Resolve(Assembly assembly)
		{
			var dependencyContext = DependencyContext.Load(assembly);

			return Resolve(assembly, dependencyContext);
		}

		internal IReadOnlyList<MetadataReference> Resolve(Assembly assembly, DependencyContext dependencyContext)
		{
			var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			IEnumerable<string> references;
			if (dependencyContext == null)
			{
				var context = new HashSet<string>();
				var x = GetReferencedAssemblies(assembly, ExcludedAssemblies, context).Union(new[] { assembly }).ToArray();
				references = x.Select(p => _pathFormatter.GetAssemblyPath(p)).ToList();
			}
			else
			{
				references = dependencyContext.CompileLibraries.Where(x => !ExcludedAssemblies.Contains(x.Name)).SelectMany(library => library.ResolveReferencePaths()).ToList();

				if (!references.Any())
				{
					throw new RazorLightException("Can't load metadata reference from the entry assembly. " +
												  "Make sure PreserveCompilationContext is set to true in *.csproj file");
				}
			}

			var metadataReferences = new List<MetadataReference>();

			foreach (var reference in references)
			{
				if (!libraryPaths.Add(reference)) continue;

				using (var stream = File.OpenRead(reference))
				{
					var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
					var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

					metadataReferences.Add(assemblyMetadata.GetReference(filePath: reference));
				}
			}

			if (AdditionalMetadataReferences.Any())
			{
				metadataReferences.AddRange(AdditionalMetadataReferences);
			}

			return metadataReferences;
		}

		private static IEnumerable<Assembly> GetReferencedAssemblies(Assembly a, ISet<string> excludedAssemblies, HashSet<string> visitedAssemblies = null)
		{
			visitedAssemblies = visitedAssemblies ?? new HashSet<string>();
			if (!visitedAssemblies.Add(a.GetName().EscapedCodeBase))
			{
				yield break;
			}

			foreach (var assemblyRef in a.GetReferencedAssemblies())
			{
				if (visitedAssemblies.Contains(assemblyRef.EscapedCodeBase)) { continue; }

				if (excludedAssemblies.Any(s => s.Contains(assemblyRef.Name))) { continue; }
				var loadedAssembly = Assembly.Load(assemblyRef);
				yield return loadedAssembly;
				foreach (var referenced in GetReferencedAssemblies(loadedAssembly, excludedAssemblies, visitedAssemblies))
				{
					yield return referenced;
				}
			}
		}

	
	}
}
