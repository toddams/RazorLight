using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RazorLight.Razor
{
    public class FileSystemRazorProject : RazorLightProject
    {
        public FileSystemRazorProject(string root)
        {
            Root = root;
        }

        public override RazorLightProjectItem GetItem(string templateKey)
        {
            string absolutePath = NormalizeAndEnsureValidPath(templateKey);

            return new FileSystemRazorProjectItem(templateKey, new FileInfo(absolutePath));
        }

        public string Root { get; }

        protected string NormalizeAndEnsureValidPath(string templateKey)
        {
            if (string.IsNullOrEmpty(templateKey))
            {
                throw new ArgumentException(nameof(templateKey));
            }

            var absolutePath = templateKey;
            if (!absolutePath.StartsWith(Root, StringComparison.OrdinalIgnoreCase))
            {
                if (templateKey[0] == '/' || templateKey[0] == '\\')
                {
                    templateKey = templateKey.Substring(1);
                }

                absolutePath = Path.Combine(Root, templateKey);
            }

            absolutePath = absolutePath.Replace('\\', '/');

            return absolutePath;
        }

        public override IEnumerable<RazorLightProjectItem> GetImports(string templateKey)
        {
            return Enumerable.Empty<RazorLightProjectItem>();
        }
    }
}
