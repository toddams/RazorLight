using System;
using System.Collections.Generic;

namespace RazorLight.Compilation
{
	public class CompilationContext
	{
		public string Content { get; }

		public CompilationContext(string content)
		{
			if (string.IsNullOrEmpty(content))
			{
				throw new ArgumentNullException(nameof(content));
			}

			this.Content = content;
		}
	}
}
