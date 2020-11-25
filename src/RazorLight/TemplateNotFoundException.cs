using System;
using System.Collections.Generic;

namespace RazorLight
{
	public class TemplateNotFoundException : RazorLightException
	{
		public TemplateNotFoundException(string message) : base(message) { }

		public TemplateNotFoundException(string message, Exception exception) : base(message, exception) { }

		public TemplateNotFoundException(
			string message,
			IEnumerable<string> knownDynamicTemplateKeys,
			IEnumerable<string> knownProjectTemplateKeys) : base(message)
		{
			KnownDynamicTemplateKeys = knownDynamicTemplateKeys;
			KnownProjectTemplateKeys = knownProjectTemplateKeys;
		}

		/// <summary>
		/// The known template keys of any dynamically created templates.
		/// Only set when <c>RazorLightOptions.DebugMode = true</c>
		/// </summary>
		public IEnumerable<string> KnownDynamicTemplateKeys { get; }

		/// <summary>
		/// The known template keys by the associated <c>RazorLightProject</c>.
		/// Only set when <c>RazorLightOptions.DebugMode = true</c>
		/// </summary>
		public IEnumerable<string> KnownProjectTemplateKeys { get; }
	}
}
