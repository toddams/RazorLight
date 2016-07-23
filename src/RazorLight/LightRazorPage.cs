using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace RazorLight
{
	/// <summary>
	/// Lightweight Razor page with a <see cref="TextWriter"/> for the generated output
	/// </summary>
	public abstract class LightRazorPage
	{
		private StringWriter _valueBuffer;
		private TextWriter _pageWriter;

		protected LightRazorPage()
		{
		}

		public HtmlEncoder HtmlEncoder { get; set; } = HtmlEncoder.Default;

		/// <summary>
		/// ExecuteAsync will write it's output to this stream
		/// </summary>
		public virtual TextWriter Output { get; set; }

		/// <summary>
		/// Gets the dynamic view data dictionary.
		/// </summary>
		
		//public dynamic ViewBag { get; set; }

		public abstract Task ExecuteAsync();

		public async Task<HtmlString> FlushAsync()
		{
			await Output.FlushAsync();
			return HtmlString.Empty;
		}

		/// <summary>
		/// Writes the specified <paramref name="value"/> with HTML encoding to <see cref="Output"/>.
		/// </summary>
		/// <param name="value">The <see cref="object"/> to write.</param>
		public virtual void Write(object value)
		{
			WriteTo(Output, value);
		}

		/// <summary>
		/// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
		/// <param name="value">The <see cref="object"/> to write.</param>
		/// <remarks>
		/// <paramref name="value"/>s of type <see cref="IHtmlContent"/> are written using
		/// <see cref="IHtmlContent.WriteTo(TextWriter, HtmlEncoder)"/>.
		/// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
		/// <paramref name="writer"/>.
		/// </remarks>
		public virtual void WriteTo(TextWriter writer, object value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			WriteTo(writer, HtmlEncoder, value);
		}

		/// <summary>
		/// Writes the specified <paramref name="value"/> with HTML encoding to given <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
		/// <param name="encoder">
		/// The <see cref="System.Text.Encodings.Web.HtmlEncoder"/> to use when encoding <paramref name="value"/>.
		/// </param>
		/// <param name="value">The <see cref="object"/> to write.</param>
		/// <remarks>
		/// <paramref name="value"/>s of type <see cref="IHtmlContent"/> are written using
		/// <see cref="IHtmlContent.WriteTo(TextWriter, HtmlEncoder)"/>.
		/// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
		/// <paramref name="writer"/>.
		/// </remarks>
		public static void WriteTo(TextWriter writer, HtmlEncoder encoder, object value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			if (encoder == null)
			{
				throw new ArgumentNullException(nameof(encoder));
			}

			if (value == null || value == HtmlString.Empty)
			{
				return;
			}

			var htmlContent = value as IHtmlContent;
			if (htmlContent != null)
			{
				htmlContent.WriteTo(writer, encoder);

				return;
			}

			WriteTo(writer, encoder, value.ToString());
		}

		/// <summary>
		/// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
		/// <param name="value">The <see cref="string"/> to write.</param>
		public virtual void WriteTo(TextWriter writer, string value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			WriteTo(writer, HtmlEncoder, value);
		}

		private static void WriteTo(TextWriter writer, HtmlEncoder encoder, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				// Perf: Encode right away instead of writing it character-by-character.
				// character-by-character isn't efficient when using a writer backed by a ViewBuffer.
				var encoded = encoder.Encode(value);
				writer.Write(encoded);
			}
		}

		/// <summary>
		/// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
		/// </summary>
		/// <param name="value">The <see cref="object"/> to write.</param>
		public virtual void WriteLiteral(object value)
		{
			WriteLiteralTo(Output, value);
		}

		/// <summary>
		/// Writes the specified <paramref name="value"/> without HTML encoding to the <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
		/// <param name="value">The <see cref="object"/> to write.</param>
		public virtual void WriteLiteralTo(TextWriter writer, object value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			if (value != null)
			{
				WriteLiteralTo(writer, value.ToString());
			}
		}

		/// <summary>
		/// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
		/// </summary>
		/// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
		/// <param name="value">The <see cref="string"/> to write.</param>
		public virtual void WriteLiteralTo(TextWriter writer, string value)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			if (!string.IsNullOrEmpty(value))
			{
				writer.Write(value);
			}
		}
	}
}
