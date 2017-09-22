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

            this.RootType = rootType;
        }

        /// <summary>
		/// The type from the assembly that contains embedded resources
		/// </summary>
		public Type RootType { get; }

        public virtual string Extension { get; set; } = ".cshtml";

        public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
        {
            templateKey = NormalizeKey(templateKey);
           

            var item = new EmbeddedRazorProjectItem(RootType, templateKey);

            return Task.FromResult((RazorLightProjectItem)item);
        }

        public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
        {
            return Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
        }

        public override string NormalizeKey(string templateKey)
        {
            if (!templateKey.EndsWith(Extension))
            {
                templateKey = templateKey + Extension;
            }

            return templateKey;
        }
    }
}
