using System;
using Xunit;

namespace RazorLight.Tests
{
    public class EngineFactoryTest
    {
	    private string viewsRoot = @"D:\MyProjects\RazorLight\tests\RazorLight.Tests\Views";

		[Fact]
	    public void PhysicalFactory_Throws_On_RootNull()
		{
			var action = new Action(() => EngineFactory.CreatePhysical(null));

			Assert.Throws<ArgumentNullException>(action);
		}
    }
}
