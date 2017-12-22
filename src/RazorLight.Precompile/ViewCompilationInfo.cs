using Microsoft.AspNetCore.Razor.Language;

namespace RazorLight.Precompile
{
    internal struct ViewCompilationInfo
    {
        public ViewCompilationInfo(
            TemplateFileInfo viewFileInfo,
            RazorCSharpDocument cSharpDocument)
        {
            TemplateFileInfo = viewFileInfo;
            CSharpDocument = cSharpDocument;
        }

        public TemplateFileInfo TemplateFileInfo { get; }

        public RazorCSharpDocument CSharpDocument { get; }
    }
}
