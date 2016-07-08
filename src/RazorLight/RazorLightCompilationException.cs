using System;
using System.Collections.Generic;

namespace RazorLight
{
	public class RazorLightCompilationException : RazorLightException
    {
		private List<string> compilationErrors;

		public IReadOnlyList<string> CompilationErrors => compilationErrors;

		public RazorLightCompilationException(string message, IEnumerable<string> errors) : base(message)
		{
			this.compilationErrors = new List<string>();
			if (errors != null)
				compilationErrors.AddRange(errors);
		}
    }
}
