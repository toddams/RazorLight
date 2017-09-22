using RazorLight.Razor;
using System;

namespace RazorLight
{
    public interface IEngineFactory
    {
        /// <summary>
        /// Creates RazorLightEngine with a filesystem razor project
        /// </summary>
        /// <param name="root">Root folder where views are stored</param>
        /// <returns></returns>
        RazorLightEngine ForFileSystem(string root);

        /// <summary>
        /// Creates RazorLightEngine with a embedded resource razor project
        /// </summary>
        /// <param name="rootType">Type of the root.</param>
        /// <returns>Instance of RazorLightEngine</returns>
        RazorLightEngine ForEmbeddedResources(Type rootType);

        /// <summary>
        ///Creates RazorLightEngine with a custom RazorLightProject
        /// </summary>
        /// <param name="project">The project</param>
        /// <returns>Instance of RazorLightEngine</returns>
        RazorLightEngine Create(RazorLightProject project, RazorLightOptions options);
    }
}