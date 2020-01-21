using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using System;
using System.Globalization;

namespace RazorLight.Internal
{
	internal static class CodeWriterExtensions
	{
		public static IDisposable BuildLinePragma(this CodeWriter writer, SourceSpan? span)
		{
			if (string.IsNullOrEmpty(span?.FilePath))
			{
				// Can't build a valid line pragma without a file path.
				return NullDisposable.Default;
			}

			return new LinePragmaWriter(writer, span.Value);
		}

		public static bool IsAtBeginningOfLine(this CodeWriter writer)
		{
			return writer.Length == 0 || writer[writer.Length - 1] == '\n';
		}

		public static CodeWriter WriteLineNumberDirective(this CodeWriter writer, SourceSpan span)
		{
			if (writer.Length >= writer.NewLine.Length && !IsAtBeginningOfLine(writer))
			{
				writer.WriteLine();
			}

			var lineNumberAsString = (span.LineIndex + 1).ToString(CultureInfo.InvariantCulture);
			return writer.Write("#line ").Write(lineNumberAsString).Write(" \"").Write(span.FilePath).WriteLine("\"");
		}

		private class LinePragmaWriter : IDisposable
		{
			private readonly CodeWriter _writer;
			private readonly int _startIndent;

			public LinePragmaWriter(CodeWriter writer, SourceSpan span)
			{
				if (writer == null)
				{
					throw new ArgumentNullException(nameof(writer));
				}

				_writer = writer;
				_startIndent = _writer.CurrentIndent;
				_writer.CurrentIndent = 0;
				WriteLineNumberDirective(writer, span);
			}

			public void Dispose()
			{
				// Need to add an additional line at the end IF there wasn't one already written.
				// This is needed to work with the C# editor's handling of #line ...
				var endsWithNewline = _writer.Length > 0 && _writer[_writer.Length - 1] == '\n';

				// Always write at least 1 empty line to potentially separate code from pragmas.
				_writer.WriteLine();

				// Check if the previous empty line wasn't enough to separate code from pragmas.
				if (!endsWithNewline)
				{
					_writer.WriteLine();
				}

				_writer
					.WriteLine("#line default")
					.WriteLine("#line hidden");

				_writer.CurrentIndent = _startIndent;
			}
		}

		private class NullDisposable : IDisposable
		{
			public static readonly NullDisposable Default = new NullDisposable();

			private NullDisposable()
			{
			}

			public void Dispose()
			{
			}
		}
	}
}
