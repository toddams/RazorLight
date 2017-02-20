using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Reflection;
#if NETSTANDARD1_6
using Microsoft.Extensions.DependencyModel;
#endif

namespace RazorLight
{
	public class UseEntryAssemblyMetadataResolver : IMetadataResolver
	{
		public IList<MetadataReference> GetMetadataReferences()
		{
			var metadataReferences = new List<MetadataReference>();

#if NETSTANDARD1_6
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
					"Make sure preserveCompilationContext is set to true in buildOptions section of project.json or in *.csproj file");
			}
#elif NET451
			//DependencyContext works also on 4.5.1, but requires new project.json project structure
			System.AppDomain.CurrentDomain
				.GetAssemblies()
				.Where(a => !a.IsDynamic
							&& System.IO.File.Exists(a.Location))
				.GroupBy(a => a.GetName().Name)
				.Select(grp => grp.First(y => y.GetName().Version == grp.Max(x => x.GetName().Version)))
				.Select(a => MetadataReference.CreateFromFile(a.Location))
				.ToList()
				.ForEach(a => metadataReferences.Add(a));

			//Ensure RuntimeBinder assembly is added to support Dynamic
			var binderReference = MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location);
			if (!metadataReferences.Contains(binderReference))
				metadataReferences.Add(binderReference);
#endif

			return metadataReferences;
		}
	}
}
