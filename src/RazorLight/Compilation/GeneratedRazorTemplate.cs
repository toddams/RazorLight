using Microsoft.AspNetCore.Razor.Language;
using System;

namespace RazorLight.Compilation
{
    public class GeneratedRazorTemplate
    {
        public GeneratedRazorTemplate(string templateKey, RazorCSharpDocument cSharpDocument)
        {
            if(string.IsNullOrEmpty(templateKey))
            {
                throw new ArgumentNullException(nameof(templateKey));
            }

            if (cSharpDocument == null)
            {
                throw new ArgumentNullException(nameof(cSharpDocument));
            }

            TemplateKey = templateKey;
            CSharpDocument = cSharpDocument;
        }

        public string TemplateKey { get; set; }

        public RazorCSharpDocument CSharpDocument { get; set; }

        public string GeneratedCode => CSharpDocument.GeneratedCode;
    }
}
