using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RazorLight.Compilation;
using Xunit;

namespace RazorLight.Tests.Compilation
{
	public class TemplateCompilationExceptionTests
	{
		[Fact]
		public void Ensure_CompilationDiagnostics_FormattedMessage_Matches_CompilationErrors()
		{
			var exception = new TemplateCompilationException("Error message", new TemplateCompilationDiagnostic[]
			{
				new TemplateCompilationDiagnostic("diagnosticMessage", "diagnosticFormattedMessage",
					new FileLinePositionSpan("path", new LinePosition(3, 1), new LinePosition(4, 2)))
			});
			
			Assert.NotEmpty(exception.CompilationDiagnostics);
			Assert.NotEmpty(exception.CompilationErrors);
			
			Assert.Equal(1, exception.CompilationDiagnostics.Count);
			Assert.Equal(1, exception.CompilationErrors.Count);

			var firstDiagnostic = exception.CompilationDiagnostics[0];
			Assert.Equal("diagnosticMessage",firstDiagnostic.ErrorMessage);
			Assert.Equal("diagnosticFormattedMessage",firstDiagnostic.FormattedMessage);
			Assert.Equal("path",firstDiagnostic.LineSpan?.Path);
			Assert.Equal(3,firstDiagnostic.LineSpan?.StartLinePosition.Line);
			Assert.Equal(1,firstDiagnostic.LineSpan?.StartLinePosition.Character);
			Assert.Equal(4,firstDiagnostic.LineSpan?.EndLinePosition.Line);
			Assert.Equal(2,firstDiagnostic.LineSpan?.EndLinePosition.Character);
			
			Assert.Equal(firstDiagnostic.FormattedMessage, exception.CompilationErrors[0]);
		}
		
		[Fact]
		public void Ensure_InitalizedWtihErrors_FormattedMessage_Matches_CompilationErrors()
		{
			var exception = new TemplateCompilationException("Error message", new string[]
			{
				"formattedMessage"
			});
			
			Assert.NotEmpty(exception.CompilationDiagnostics);
			Assert.NotEmpty(exception.CompilationErrors);
			
			Assert.Equal(1, exception.CompilationDiagnostics.Count);
			Assert.Equal(1, exception.CompilationErrors.Count);

			var firstDiagnostic = exception.CompilationDiagnostics[0];
			Assert.Equal("formattedMessage",firstDiagnostic.ErrorMessage);
			Assert.Equal("formattedMessage",firstDiagnostic.FormattedMessage);
			Assert.Null(firstDiagnostic.LineSpan);

			Assert.Equal(firstDiagnostic.FormattedMessage, exception.CompilationErrors[0]);
		}
	}
}