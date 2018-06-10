using System;

namespace RazorLight
{
    public class TemplateNotFoundException : RazorLightException
    {
		public TemplateNotFoundException(string message) : base(message) { }

		public TemplateNotFoundException(string message, Exception exception) : base(message, exception) { }
	}
}
