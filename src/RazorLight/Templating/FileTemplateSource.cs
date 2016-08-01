using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using RazorLight.Abstractions;

namespace RazorLight.Templating
{
    public class FileTemplateSource : ITemplateSource
    {
	    private string template;

	    public string Template
	    {
		    get
		    {
			    if (string.IsNullOrEmpty(template))
			    {
				    using (var reader = new StreamReader(FileInfo.CreateReadStream()))
				    {
					    this.template = reader.ReadToEnd();
				    }
			    }

			    return template;
		    }
	    }
	    public string TemplateFile { get; }

	    public IFileInfo FileInfo { get; private set; }

	    public FileTemplateSource(IFileInfo fileInfo)
	    {
		    if(fileInfo == null)
		    {
			    throw new ArgumentNullException(nameof(fileInfo));
		    }

		    this.TemplateFile = fileInfo.PhysicalPath;
		    this.FileInfo = fileInfo;
	    }

	    public TextReader CreateReader()
	    {
		    return new StringReader(Template);
	    }
    }
}
