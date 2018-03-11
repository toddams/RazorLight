using System;
using System.Collections.Generic;
using System.Text;

namespace RazorLight.ViewFeatures
{
	/// <summary>
	/// Enumeration for the kinds of <see cref="ModelMetadata"/>
	/// </summary>
	public enum ModelMetadataKind
	{
		/// <summary>
		/// Used for <see cref="ModelMetadata"/> for a <see cref="System.Type"/>.
		/// </summary>
		Type,

		/// <summary>
		/// Used for <see cref="ModelMetadata"/> for a property.
		/// </summary>
		Property,

		/// <summary>
		/// Used for <see cref="ModelMetadata"/> for a parameter.
		/// </summary>
		Parameter,
	}
}
