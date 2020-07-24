using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RazorLight.Caching;
using RazorLight.Razor;
using System;
using System.Collections.Generic;
using System.Text;

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
			if (String.IsNullOrEmpty(extension))
			{
				project = new FileSystemRazorProject(root);
			}
			else
			{
				project = new FileSystemRazorProject(root, extension);
			}

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
			_services.AddSingleton<RazorLightProject>(project);
			return this;
		}


	}
}
