using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace RazorLight
{
	/// <summary>
	/// Use this interface to provide reference metadata resolver 
	/// to resolve dependencies while compiling razor templates
	/// </summary>
	public interface IMetadataResolver
	{
		/// <summary>
		/// Returns a list of <see cref="MetadataReference"/> to resolve dependencies while compiling razor templates
		/// </summary>
		IList<MetadataReference> GetMetadataReferences();
	}
}
