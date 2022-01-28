using RazorLight.Razor;
using System;

namespace RazorLight
{
	[Obsolete("Use RazorLightEngineBuilder instead", true)]
	public interface IEngineFactory
	{
		/// <summary>
		/// Creates RazorLightEngine with a filesystem razor project
		/// </summary>
		/// <param name="root">Root folder where views are stored</param>
		/// <returns></returns>
		RazorLightEngine ForFileSystem(string root);

		/// <summary>
		/// Creates RazorLightEngine with a filesystem razor project
		/// </summary>
		/// <param name="root">Root folder where views are stored</param>
		/// <param name="options">Engine options</param>
		/// <returns>Instance of RazorLightEngine</returns>
		RazorLightEngine ForFileSystem(string root, RazorLightOptions options);

		/// <summary>
		/// Creates RazorLightEngine with a embedded resource razor project
		/// </summary>
		/// <param name="rootType">Type of the root.</param>
		/// <returns>Instance of RazorLightEngine</returns>
		RazorLightEngine ForEmbeddedResources(Type rootType);

		/// <summary>
		/// Creates RazorLightEngine with a embedded resource razor project
		/// </summary>
		/// <param name="rootType">Type of the root.</param>
		/// <param name="options">Engine options</param>
		/// <returns>Instance of RazorLightEngine</returns>
		RazorLightEngine ForEmbeddedResources(Type rootType, RazorLightOptions options);

		RazorLightEngine Create(RazorLightOptions options = null);

		/// <summary>
		/// Creates RazorLightEngine with a custom RazorLightProject
		/// </summary>
		/// <param name="project">The project</param>
		/// <param name="options">Options for configuring the RazorLightEngine.</param>
		/// <returns>Instance of RazorLightEngine</returns>
		RazorLightEngine Create(RazorLightProject project, RazorLightOptions options);
	}
}