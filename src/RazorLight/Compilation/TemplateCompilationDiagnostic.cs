using Microsoft.CodeAnalysis;

namespace RazorLight.Compilation
{
	public class TemplateCompilationDiagnostic
	{
		public string ErrorMessage { get; }
		public string FormattedMessage { get; }
		public FileLinePositionSpan? LineSpan { get; }

		public TemplateCompilationDiagnostic(string errorMessage, string formattedMessage, FileLinePositionSpan? lineSpan)
		{
			ErrorMessage = errorMessage;
			FormattedMessage = formattedMessage;
			LineSpan = lineSpan;
		}
	}
}