using System;
using RazorLight.Razor;

namespace RazorLight
{
	[Obsolete("Use RazorLightEngineBuilder instead", true)]
	public class EngineFactory : IEngineFactory
	{
		/// <summary>
		/// Creates RazorLightEngine with a filesystem razor project
		/// </summary>
		/// <param name="root">Root folder where views are stored</param>
		/// <returns>Instance of RazorLightEngine</returns>
		public virtual RazorLightEngine ForFileSystem(string root)
		{
			var project = new FileSystemRazorProject(root);

			return Create(project);
		}

		/// <summary>
		/// Creates RazorLightEngine with a filesystem razor project
		/// </summary>
		/// <param name="root">Root folder where views are stored</param>
		/// <param name="options">Engine options</param>
		/// <returns>Instance of RazorLightEngine</returns>
		public virtual RazorLightEngine ForFileSystem(string root, RazorLightOptions options)
		{
			var project = new FileSystemRazorProject(root);

			return Create(project, options);
		}

		/// <summary>
		/// Creates RazorLightEngine with a embedded resource razor project
		/// </summary>
		/// <param name="rootType">Type of the root.</param>
		/// <returns>Instance of RazorLightEngine</returns>
		public virtual RazorLightEngine ForEmbeddedResources(Type rootType)
		{
			var project = new EmbeddedRazorProject(rootType);

			return Create(project);
		}

		/// <summary>
		/// Creates RazorLightEngine with a embedded resource razor project
		/// </summary>
		/// <param name="rootType">Type of the root.</param>
		/// <param name="options">Engine options</param>
		/// <returns>Instance of RazorLightEngine</returns>
		public virtual RazorLightEngine ForEmbeddedResources(Type rootType, RazorLightOptions options)
		{
			var project = new EmbeddedRazorProject(rootType);

			return Create(project, options);
		}

		public RazorLightEngine Create(RazorLightOptions options = null)
		{
			return Create(null, options);
		}

		/// <summary>
		/// Creates RazorLightEngine with a custom RazorLightProject
		/// </summary>
		/// <param name="project">The project</param>
		/// <param name="options">Options for configuring the RazorLightEngine.</param>
		/// <returns>Instance of RazorLightEngine</returns>
		public virtual RazorLightEngine Create(RazorLightProject project, RazorLightOptions options = null)
		{
			return null;
		}
	}
}
