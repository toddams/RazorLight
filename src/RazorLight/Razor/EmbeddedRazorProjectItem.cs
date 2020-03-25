using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RazorLight.Razor
{
	public class EmbeddedRazorProjectItem : RazorLightProjectItem
	{
		private readonly string fullTemplateKey;

		public EmbeddedRazorProjectItem(Assembly assembly, string rootNamespace, string key)
		{
			Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
			Key = key ?? throw new ArgumentNullException(nameof(key));

			if (rootNamespace == null)
			{
				rootNamespace = "";
			}

			if (!string.IsNullOrEmpty(rootNamespace) && !rootNamespace.EndsWith("."))
				rootNamespace += ".";

			this.fullTemplateKey = rootNamespace + key;
		}

		public EmbeddedRazorProjectItem(Type rootType, string key)
		{
			if (rootType == null)
			{
				throw new ArgumentNullException(nameof(rootType));
			}

			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			Key = key;
			Assembly = rootType.GetTypeInfo().Assembly;

			this.fullTemplateKey = $"{rootType.Namespace}.{Key}";
		}

		public Assembly Assembly { get; set; }

		public override string Key { get; }

		public override bool Exists
		{
			get
			{
				return Assembly.GetManifestResourceNames().Any(f => f == this.fullTemplateKey);
			}
		}

		public override Stream Read()
		{
			return Assembly.GetManifestResourceStream(this.fullTemplateKey);
		}
	}
}
