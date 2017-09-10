using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RazorLight.Razor
{
    /// <summary>
    /// Specifies RazorProject where templates are located in files
    /// </summary>
    public class FileSystemRazorProject : RazorLightProject
    {
        public FileSystemRazorProject(string root)
        {
            Root = root;
        }

        public virtual string FileExtension { get; set; } = ".cshtml";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateKey"></param>
        /// <returns></returns>
        /// <remarks>Can not use Task.FromResult as Task<T> is not covariant</remarks>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            string absolutePath = NormalizeAndEnsureValidPath(templateKey);
            var item = new FileSystemRazorProjectItem(templateKey, new FileInfo(absolutePath));

            return Task.FromResult((RazorLightProjectItem)item);
        }

        /// <summary>
        /// Root folder
        /// </summary>
        public string Root { get; }

        protected string NormalizeAndEnsureValidPath(string templateKey)
        {
            if (string.IsNullOrEmpty(templateKey))
            {
                throw new ArgumentException(nameof(templateKey));
            }

            if(!templateKey.EndsWith(FileExtension))
            {
                templateKey = templateKey + FileExtension;
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

        public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
        {
            return Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
        }
    }
}
