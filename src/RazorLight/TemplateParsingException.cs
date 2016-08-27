using System.Collections.Generic;
using Microsoft.AspNetCore.Razor;

namespace RazorLight
{
	public class TemplateParsingException : RazorLightException
	{
		public IReadOnlyList<RazorError> ParserErrors { get; private set; }

		public TemplateParsingException(string message, IEnumerable<RazorError> parserErrors) : base(message)
		{
			this.ParserErrors = new List<RazorError>(parserErrors);
		}
	}
}
