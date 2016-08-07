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
			string root = @"D:\MyProjects\RazorLight\sandbox\Sandbox\Views";

			var engine = EngineFactory.CreatePhysical(root);

			string result = engine.Parse("Test.cshtml", new Sandbox.TestViewModel());

			Assert.NotNull(result);
		}
    }
}
