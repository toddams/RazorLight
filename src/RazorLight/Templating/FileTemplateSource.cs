using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using RazorLight.Abstractions;
using RazorLight.Internal;

namespace RazorLight.Templating
{
    public class FileTemplateSource : ITemplateSource
    {
	    private string template;

		public FileTemplateSource(IFileInfo fileInfo, string relativeFilePath)
		{
			if (fileInfo == null)
			{
				throw new ArgumentNullException(nameof(fileInfo));
			}

			if (string.IsNullOrEmpty(relativeFilePath))
			{
				throw new ArgumentNullException(nameof(relativeFilePath));
			}

			this.FileInfo = fileInfo;
			this.IsPhysicalPage = true;
			this.TemplateFile = PathNormalizer.GetNormalizedPath(fileInfo.PhysicalPath);
			this.TemplateKey = relativeFilePath;
		}

		public string Template
	    {
		    get
		    {
			    if (string.IsNullOrEmpty(template))
			    {
				    using (var reader = CreateReader())
				    {
					    template = reader.ReadToEnd();
				    }
			    }

			    return template;
		    }
	    }
	    public string TemplateFile { get; }
	    public bool IsPhysicalPage { get; }
	    public string TemplateKey { get; }

	    public IFileInfo FileInfo { get; private set; }

	    
	    public TextReader CreateReader()
	    {
			return new StreamReader(FileInfo.CreateReadStream());
	    }
    }
}

