using Microsoft.AspNetCore.Razor.Language;
using System.Collections.Generic;

namespace RazorLight.Generation
{
	public class TemplateGenerationException : RazorLightException
	{
		public TemplateGenerationException(string message, IReadOnlyList<RazorDiagnostic> diagnostic) : base(message)
		{
			Diagnostics = diagnostic;
		}

		public IReadOnlyList<RazorDiagnostic> Diagnostics { get; set; }
	}
}
