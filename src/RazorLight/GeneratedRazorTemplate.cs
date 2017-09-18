using Microsoft.AspNetCore.Razor.Language;
using System;

namespace RazorLight
{
    public class GeneratedRazorTemplate
    {
        public GeneratedRazorTemplate(string templateKey, RazorCSharpDocument razorCSharpDocument)
        {
            if(string.IsNullOrEmpty(templateKey))
            {
                throw new ArgumentNullException(nameof(templateKey));
            }

            if (razorCSharpDocument == null)
            {
                throw new ArgumentNullException(nameof(templateKey));
            }

            TemplateKey = templateKey;
            CSharpDocument = razorCSharpDocument;
        }

        public string TemplateKey { get; set; }

        public RazorCSharpDocument CSharpDocument { get; set; }

        public string GeneratedCode => CSharpDocument.GeneratedCode;
    }
}
