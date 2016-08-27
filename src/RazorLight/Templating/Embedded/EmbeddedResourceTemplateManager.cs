using System;
using System.IO;
using System.Reflection;

namespace RazorLight.Templating.Embedded
{
	public class EmbeddedResourceTemplateManager : ITemplateManager
	{
		/// <summary>
		/// Initializes a new TemplateManager.
		/// </summary>
		/// <param name="rootType">The type from the assembly that contains embedded resources that will act as a root type for Assembly.GetManifestResourceStream() calls.</param>
		public EmbeddedResourceTemplateManager(Type rootType)
		{
			if (rootType == null)
			{
				throw new ArgumentNullException(nameof(rootType));
			}

			this.RootType = rootType;
		}

		/// <summary>
		/// The type from the assembly that contains embedded resources
		/// </summary>
		public Type RootType { get; }

		public ITemplateSource Resolve(string key)
		{
			Assembly assembly = this.RootType.GetTypeInfo().Assembly;

			using (var stream = assembly.GetManifestResourceStream(this.RootType.Namespace + "." + key + ".cshtml"))
			{
				if (stream == null)
				{
					throw new RazorLightException(string.Format("Couldn't load resource '{0}.{1}.cshtml' from assembly {2}", this.RootType.Namespace, key, this.RootType.AssemblyQualifiedName));
				}

				using (var reader = new StreamReader(stream))
				{
					return new LoadedTemplateSource(reader.ReadToEnd());
				}
			}
		}
	}
}
