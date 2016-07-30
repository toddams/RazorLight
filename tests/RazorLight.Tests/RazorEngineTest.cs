using System;
using System.IO;
using Xunit;

namespace RazorLight.Tests
{
	public class RazorEngineTest
	{
		private string viewFolder = @"D:\MyProjects\RazorLight\tests\RazorLight.Tests\Views";

		[Fact]
		public void Throw_If_ViewFolder_Not_Set()
		{
			var engine = new RazorLightEngine();

			string result = engine.ParseFile("Views/Test.cshtml", new TestViewModel());

			Assert.Equal(result, string.Empty);
		}

		//[Fact]
		//public void Throw_IfViewNotFound()
		//{
		//	var options = new ConfigurationOptions()
		//	{
		//		ViewsFolder = viewFolder
		//	};

		//	var engine = new RazorLightEngine(options);
		//	string result = null;

		//	Action action = () => { result = engine.ParseFile("ggg.cshtml", new TestViewModel()); };

		//	Assert.Throws<FileNotFoundException>(action);
		//}
	}
}
