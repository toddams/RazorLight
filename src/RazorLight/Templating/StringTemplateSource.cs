using System;
using System.IO;

namespace RazorLight.Templating
{
    public class StringTemplateSource : ITemplateSource
    {
	    public string Content { get; private set; }
	    public string FilePath { get; private set; }
	    public string TemplateKey { get; }

	    public StringTemplateSource(string content)
	    {
		    if (string.IsNullOrEmpty(content))
		    {
			    throw new ArgumentNullException(nameof(content));
		    }

		    this.Content = content;
		    this.TemplateKey = GetRandomString();
		    this.FilePath = null;
	    }

	    private string GetRandomString()
	    {
		    return Path.GetFileName(Path.GetRandomFileName());
	    }

	    public TextReader CreateReader()
	    {
			return new StringReader(Content);
	    }
    }
}
