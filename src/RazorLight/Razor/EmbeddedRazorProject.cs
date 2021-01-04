using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RazorLight.Razor
{
	public class EmbeddedRazorProject : RazorLightProject
	{
		public EmbeddedRazorProject(Type rootType)
		{
			if (rootType == null)
			{
				throw new ArgumentNullException(nameof(rootType));
			}

			Assembly = rootType.Assembly;
			RootNamespace = rootType.Namespace;
		}

		public EmbeddedRazorProject(Assembly assembly, string rootNamespace = "")
		{
			Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));

			RootNamespace = rootNamespace;
		}

		public Assembly Assembly { get; set; }

		public string RootNamespace { get; set; }

		public string Extension { get; set; } = ".cshtml";

		public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
		{
			if (string.IsNullOrEmpty(templateKey))
			{
				throw new ArgumentNullException(nameof(templateKey));
			}

			if (!templateKey.EndsWith(Extension))
			{
				templateKey += Extension;
			}

			var item = new EmbeddedRazorProjectItem(Assembly, RootNamespace, templateKey);

			return Task.FromResult((RazorLightProjectItem)item);
		}

		public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
		{
			return Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
		}

		public override Task<IEnumerable<string>> GetKnownKeysAsync()
		{
			var ignoredPrefix = string.IsNullOrEmpty(RootNamespace) ? Assembly.GetName().FullName : RootNamespace;
			if (!ignoredPrefix.EndsWith(".")) ignoredPrefix += ".";

			var fullResourceNames = this.Assembly.GetManifestResourceNames()
				.Where(x => x.StartsWith(ignoredPrefix) && x.EndsWith(Extension));

			var keys = fullResourceNames
				.Select(x => x.Remove(0, ignoredPrefix.Length)) // Remove prefix
				.Select(x => x.Remove(x.Length - Extension.Length, Extension.Length)); // Remove extension

			return Task.FromResult(keys);
		}
	}
}
