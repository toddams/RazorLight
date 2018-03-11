using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using RazorLight.Internal;

namespace RazorLight.ViewFeatures.Rendering
{
	/// <summary>
	/// An HTML form element in an MVC view.
	/// </summary>
	public class MvcForm : IDisposable
	{
		private readonly PageContext _viewContext;
		private readonly HtmlEncoder _htmlEncoder;

		private bool _disposed;

		/// <summary>
		/// Initializes a new instance of <see cref="MvcForm"/>.
		/// </summary>
		/// <param name="viewContext">The <see cref="ViewContext"/>.</param>
		/// <param name="htmlEncoder">The <see cref="HtmlEncoder"/>.</param>
		public MvcForm(PageContext viewContext, HtmlEncoder htmlEncoder)
		{
			if (viewContext == null)
			{
				throw new ArgumentNullException(nameof(viewContext));
			}

			if (htmlEncoder == null)
			{
				throw new ArgumentNullException(nameof(htmlEncoder));
			}

			_viewContext = viewContext;
			_htmlEncoder = htmlEncoder;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				GenerateEndForm();
			}
		}

		/// <summary>
		/// Renders the &lt;/form&gt; end tag to the response.
		/// </summary>
		public void EndForm()
		{
			Dispose();
		}

		/// <summary>
		/// Renders <see cref="ViewFeatures.FormContext.EndOfFormContent"/> and
		/// the &lt;/form&gt;.
		/// </summary>
		protected virtual void GenerateEndForm()
		{
			RenderEndOfFormContent();
			_viewContext.Writer.Write("</form>");
			_viewContext.FormContext = new FormContext();
		}

		private void RenderEndOfFormContent()
		{
			var formContext = _viewContext.FormContext;
			if (!formContext.HasEndOfFormContent)
			{
				return;
			}

			var viewBufferWriter = _viewContext.Writer as ViewBufferTextWriter;
			if (viewBufferWriter == null)
			{
				foreach (var content in formContext.EndOfFormContent)
				{
					content.WriteTo(_viewContext.Writer, _htmlEncoder);
				}
			}
			else
			{
				foreach (var content in formContext.EndOfFormContent)
				{
					viewBufferWriter.Write(content);
				}
			}
		}
	}
}
