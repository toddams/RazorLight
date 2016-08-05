using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Xunit;

namespace RazorLight.Tests
{
    public class UseEntryAssemblyResolverTest
    {
		[Fact]
	    public void Entry_Assembly_Resolver_Has_NonEmpty_Output()
	    {
		    var resolver = new UseEntryAssemblyMetadataResolver();

		    IList<MetadataReference> references = resolver.GetMetadataReferences();

			Assert.NotNull(references);
			Assert.NotEmpty(references);
	    }
    }
}
