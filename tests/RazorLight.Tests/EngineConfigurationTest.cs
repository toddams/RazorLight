using Xunit;

namespace RazorLight.Tests
{
    public class EngineConfigurationTest
    {
		[Fact]
	    public void Default_Configuration_Has_No_Namespaces()
	    {
		    var config = EngineConfiguration.Default;

		    int expectedNamespaces = 0;

			Assert.Equal(config.Namespaces.Count, expectedNamespaces);
	    }

		[Fact]
	    public void Default_Configuration_No_Null_Properties()
	    {
		    var config = EngineConfiguration.Default;

			Assert.NotNull(config.Activator);
			Assert.NotNull(config.CompilerService);
			Assert.NotNull(config.RazorTemplateCompiler);
	    }
    }
}
