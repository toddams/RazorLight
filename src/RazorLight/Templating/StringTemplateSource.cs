using System;
using System.IO;
using RazorLight.Abstractions;

namespace RazorLight.Templating
{
    public class StringTemplateSource : ITemplateSource
    {
	    public string Template { get; private set; }
	    public string TemplateFile { get; private set; }

	    public StringTemplateSource(string content)
	    {
		    if (string.IsNullOrEmpty(content))
		    {
			    throw new ArgumentNullException(nameof(content));
		    }

		    this.Template = content;
	    }

		public TextReader CreateReader()
	    {
			return new StringReader(Template);
	    }
    }
}
