using System.IO;

namespace RazorLight.Templating
{
	public interface ITemplateSource
	{
		string Content { get; }

		string FilePath { get; }

		string TemplateKey { get; }

		TextReader CreateReader();
	}
}
