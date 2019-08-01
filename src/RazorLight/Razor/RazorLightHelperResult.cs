using Microsoft.AspNetCore.Html;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace RazorLight.Razor
{
	public class RazorLightHelperResult : IHtmlContent
	{
		private readonly Func<TextWriter, Task> _writeAction;

		public RazorLightHelperResult(Func<TextWriter, Task> asyncAction)
		{
			_writeAction = asyncAction ?? throw new ArgumentNullException(nameof(asyncAction));
		}

		public virtual void WriteTo(TextWriter writer, HtmlEncoder encoder)
		{
			if (writer == null) throw new ArgumentNullException(nameof(writer));

			if (encoder == null) throw new ArgumentNullException(nameof(encoder));

			_writeAction(writer).GetAwaiter().GetResult();
		}
	}
}
