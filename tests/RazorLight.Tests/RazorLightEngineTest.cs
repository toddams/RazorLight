using Xunit;

namespace RazorLight.Tests
{
    public class RazorLightEngineTest
    {
		[Fact]
	    public void Can_Parse_Embeded_Resource()
	    {
		    var engine = EngineFactory.CreateEmbedded(typeof(Sandbox.TestViewModel));

		    string result = engine.Parse("Views.Test", new Sandbox.TestViewModel());

			Assert.NotNull(result);
	    }

		[Fact]
	    public void Can_Parse_Physical_Files()
		{
		    string root = PathUtility.GetViewsPath();

            var engine = EngineFactory.CreatePhysical(root);

			string result = engine.Parse("Test.cshtml", new TestViewModel());

			Assert.NotNull(result);
		}
    }
}
