using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Razor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RazorLight
{
	public class RazorLightDependencyBuilder
	{
		IServiceCollection _services;
		public RazorLightDependencyBuilder(IServiceCollection services)
		{
			_services = services;
		}

		public RazorLightDependencyBuilder UseFileSystemProject(string root, string extension = null)
		{
			_services.RemoveAll<RazorLightProject>();

			RazorLightProject project;
			if (string.IsNullOrEmpty(extension))
			{
				project = new FileSystemRazorProject(root);
			}
			else
			{
				project = new FileSystemRazorProject(root, extension);
			}

			// ReSharper disable once RedundantTypeArgumentsOfMethod
			_services.AddSingleton<RazorLightProject>(project);
			return this;
		}

		public RazorLightDependencyBuilder UseMemoryCachingProvider()
		{
			_services.RemoveAll<ICachingProvider>();
			_services.AddSingleton<ICachingProvider, MemoryCachingProvider>();
			return this;
		}

		public RazorLightDependencyBuilder UseEmbeddedResourcesProject(Type rootType)
		{
			_services.RemoveAll<RazorLightProject>();
			RazorLightProject project = new EmbeddedRazorProject(rootType);
			// ReSharper disable once RedundantTypeArgumentsOfMethod
			_services.AddSingleton<RazorLightProject>(project);
			return this;
		}

		public RazorLightDependencyBuilder UseNetFrameworkLegacyFix()
		{
			_services.RemoveAll<IAssemblyPathFormatter>();
			IAssemblyPathFormatter formatter = new LegacyFixAssemblyPathFormatter();
			// ReSharper disable once RedundantTypeArgumentsOfMethod
			_services.AddSingleton<IAssemblyPathFormatter>(formatter);
			return this;
		}

		public RazorLightDependencyBuilder SetOperatingAssembly(Assembly assembly)
		{
			_services.Configure<RazorLightOptions>(x => x.OperatingAssembly = assembly);
			return this;
		}

		public RazorLightDependencyBuilder ExcludeAssemblies(params string[] assemblyNames)
		{
			var excludedAssemblies = new HashSet<string>();

			foreach (var assemblyName in assemblyNames)
			{
				excludedAssemblies.Add(assemblyName);
			}

			_services.Configure<RazorLightOptions>(x => x.ExcludedAssemblies = excludedAssemblies);
			return this;
		}

		public RazorLightDependencyBuilder AddMetadataReferences(params MetadataReference[] metadata)
		{
			var metadataReferences = new HashSet<MetadataReference>();

			foreach (var reference in metadata)
			{
				metadataReferences.Add(reference);
			}
			_services.Configure<RazorLightOptions>(x => x.AdditionalMetadataReferences = metadataReferences);
			return this;
		}
	}
}
