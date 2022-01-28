using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RazorLight.Compilation
{
	public class TemplateCompilationException : RazorLightException
	{
		private readonly List<TemplateCompilationDiagnostic> compilationDiagnostics = new List<TemplateCompilationDiagnostic>();

		public IReadOnlyList<string> CompilationErrors => compilationDiagnostics?.Select(x => x.FormattedMessage).ToList() ?? new List<string>();

		public IReadOnlyList<TemplateCompilationDiagnostic> CompilationDiagnostics => compilationDiagnostics;

		[Obsolete("Use constructor that takes enumerable of TemplateCompilationDiagnostic as input parameters")]
		public TemplateCompilationException(string message, IEnumerable<string> errors) : base(message)
		{
			if (errors != null)
			{
				compilationDiagnostics.AddRange(errors.Select(x => new TemplateCompilationDiagnostic(x, x, null)));	
			}
		}
		
		public TemplateCompilationException(string message, IEnumerable<TemplateCompilationDiagnostic> diagnostics) : base(message)
		{
			if (diagnostics != null)
			{
				compilationDiagnostics.AddRange(diagnostics);
			}
		}
	}
}
