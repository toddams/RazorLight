using Microsoft.AspNetCore.Razor.Language;
using RazorLight.Razor;

namespace RazorLight.Generation
{
	public interface IGeneratedRazorTemplate
	{
		string TemplateKey { get; }

		string GeneratedCode { get; }

		RazorLightProjectItem ProjectItem { get; set; }
	}
}