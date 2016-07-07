using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace RazorLight
{
	public abstract class LightRazorPage
	{
		private StringWriter _valueBuffer;
		private TextWriter _pageWriter;

		public LightRazorPage()
		{
		}

		public HtmlEncoder HtmlEncoder { get; set; } = HtmlEncoder.Default;

		public virtual TextWriter Output { get; set; }

		/// <summary>
		/// Gets the dynamic view data dictionary.
		/// </summary>
		//public dynamic ViewBag { get; set; }

		public abstract Task ExecuteAsync();

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

		private void WriteUnprefixedAttributeValueTo(TextWriter writer, object value, bool isLiteral)
		{
			var stringValue = value as string;

			// The extra branching here is to ensure that we call the Write*To(string) overload where possible.
			if (isLiteral && stringValue != null)
			{
				WriteLiteralTo(writer, stringValue);
			}
			else if (isLiteral)
			{
				WriteLiteralTo(writer, value);
			}
			else if (stringValue != null)
			{
				WriteTo(writer, stringValue);
			}
			else
			{
				WriteTo(writer, value);
			}
		}

		public async Task<HtmlString> FlushAsync()
		{
			await Output.FlushAsync();
			return HtmlString.Empty;
		}

		private bool IsBoolFalseOrNullValue(string prefix, object value)
		{
			return string.IsNullOrEmpty(prefix) &&
				(value == null ||
				(value is bool && !(bool)value));
		}

		private bool IsBoolTrueWithEmptyPrefixValue(string prefix, object value)
		{
			// If the value is just the bool 'true', use the attribute name as the value.
			return string.IsNullOrEmpty(prefix) &&
				(value is bool && (bool)value);
		}

		private struct AttributeInfo
		{
			public AttributeInfo(
				string name,
				string prefix,
				int prefixOffset,
				string suffix,
				int suffixOffset,
				int attributeValuesCount)
			{
				Name = name;
				Prefix = prefix;
				PrefixOffset = prefixOffset;
				Suffix = suffix;
				SuffixOffset = suffixOffset;
				AttributeValuesCount = attributeValuesCount;

				Suppressed = false;
			}

			public int AttributeValuesCount { get; }

			public string Name { get; }

			public string Prefix { get; }

			public int PrefixOffset { get; }

			public string Suffix { get; }

			public int SuffixOffset { get; }

			public bool Suppressed { get; set; }
		}
	}

}
