using System;

namespace RazorLight
{
	public interface IEngineFactory {
		/// <summary>
		/// Creates a <see cref="IRazorLightEngine"/> that resolves templates by searching them on physical storage
		/// and tracks file changes with <seealso cref="System.IO.FileSystemWatcher"/>
		/// </summary>
		/// <param name="root">Root folder where views are stored</param>
		IRazorLightEngine CreatePhysical(string root);

		/// <summary>
		/// Creates a <see cref="IRazorLightEngine"/> that resolves templates by searching 
		/// them on physical storage with a given <see cref="IEngineConfiguration"/>
		/// and tracks file changes with <seealso cref="System.IO.FileSystemWatcher"/>
		/// </summary>
		/// <param name="root">Root folder where views are stored</param>
		/// <param name="configuration">Engine configuration</param>
		IRazorLightEngine CreatePhysical(string root, IEngineConfiguration configuration);

		/// <summary>
		/// Creates a <see cref="IRazorLightEngine"/> that resolves templates inside given type assembly as a EmbeddedResource
		/// </summary>
		/// <param name="rootType">Root type where resource is located</param>
		IRazorLightEngine CreateEmbedded(Type rootType);

		/// <summary>
		/// Creates a <see cref="IRazorLightEngine"/> that resolves templates inside given type assembly as a EmbeddedResource
		/// with a given <see cref="IEngineConfiguration"/>
		/// </summary>
		/// <param name="rootType">Root type where resource is located</param>
		/// <param name="configuration">Engine configuration</param>
		IRazorLightEngine CreateEmbedded(Type rootType, IEngineConfiguration configuration);
	}
}