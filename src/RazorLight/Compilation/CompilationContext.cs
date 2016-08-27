using System;
using System.Collections.Generic;

namespace RazorLight.Compilation
{
	public class CompilationContext
	{
		public string Content { get; }

		public ISet<string> IncludeNamespaces { get; }

		public CompilationContext(string content, ISet<string> includeNamespaces)
		{
			if (string.IsNullOrEmpty(content))
			{
				throw new ArgumentNullException(nameof(content));
			}

			this.Content = content;
			this.IncludeNamespaces = includeNamespaces ?? new HashSet<string>();
		}
	}
}
