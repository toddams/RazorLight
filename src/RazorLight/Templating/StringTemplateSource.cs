using System;
using System.IO;
using RazorLight.Abstractions;

namespace RazorLight.Templating
{
    public class StringTemplateSource : ITemplateSource
    {
	    public string Template { get; private set; }
	    public string TemplateFile { get; private set; }
	    public bool IsPhysicalPage { get; }
	    public string TemplateKey { get; }

	    public StringTemplateSource(string content)
	    {
		    if (string.IsNullOrEmpty(content))
		    {
			    throw new ArgumentNullException(nameof(content));
		    }

		    this.Template = content;
		    this.IsPhysicalPage = false;
		    this.TemplateKey = GetRandomString();
		    this.TemplateFile = null;
	    }

	    private string GetRandomString()
	    {
		    return Path.GetFileName(Path.GetRandomFileName());
	    }

	    public TextReader CreateReader()
	    {
			return new StringReader(Template);
	    }
    }
}
