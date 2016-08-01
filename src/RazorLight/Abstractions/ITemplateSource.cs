using System.IO;

namespace RazorLight.Abstractions
{
    public interface ITemplateSource
    {
		string Template { get; }

		string TemplateFile { get; }

	    TextReader CreateReader();
    }
}
