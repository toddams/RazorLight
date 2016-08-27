using System.Collections.Generic;

namespace RazorLight
{
	public class TemplateCompilationException : RazorLightException
	{
		private readonly List<string> compilationErrors;

		public IReadOnlyList<string> CompilationErrors => compilationErrors;

		public TemplateCompilationException(string message, IEnumerable<string> errors) : base(message)
		{
			this.compilationErrors = new List<string>();
			if (errors != null)
				compilationErrors.AddRange(errors);
		}
	}
}
