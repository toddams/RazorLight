using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

			this.Assembly = rootType.Assembly;
        }

		public EmbeddedRazorProject(Assembly assembly, string rootNamespace = "")
		{
			Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));

			RootNamespace = rootNamespace;
		}

		public Assembly Assembly { get; set; }

		public string RootNamespace { get; set; }

		public virtual string Extension { get; set; } = ".cshtml";

		public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
		{
			if (string.IsNullOrEmpty(templateKey))
			{
				throw new ArgumentNullException(nameof(templateKey));
			}

			if (!templateKey.EndsWith(Extension))
			{
				templateKey = templateKey + Extension;
			}

            var item = new EmbeddedRazorProjectItem(Assembly, RootNamespace, templateKey);

            return Task.FromResult((RazorLightProjectItem)item);
        }

        public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
        {
            return Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
        }
    }
}
