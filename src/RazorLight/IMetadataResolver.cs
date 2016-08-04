using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace RazorLight
{
    public interface IMetadataResolver
    {
	    IList<MetadataReference> GetMetadataReferences();
    }
}
