using System;
using System.IO;
using Xunit;

namespace RazorLight.Tests
{
	public class RazorEngineTest
	{
		private string viewFolder = @"D:\MyProjects\RazorLight\tests\RazorLight.Tests\Views";

		[Fact]
	    public void ParseFileThrows_IfViewFolderNotSet()
	    {
		    var engine = new RazorLightEngine();

		    Action action = () => { engine.ParseFile("Views/Test.cshtml", new TestViewModel()); };

		    Assert.Throws<RazorLightException>(action);
	    }

		[Fact]
	    public void CanParseFile_TrueIfViewFolderIsSet()
		{
			var options = new ConfigurationOptions()
			{
				ViewsFolder = viewFolder
			};

			var engine = new RazorLightEngine(options);

			Assert.True(engine.CanParseFiles);
		}

		[Fact]
		public void Throw_IfViewNotFound()
		{
			var options = new ConfigurationOptions()
			{
				ViewsFolder = viewFolder
			};

			var engine = new RazorLightEngine(options);
			string result = null;

			Action action = () => { result = engine.ParseFile("ggg.cshtml", new TestViewModel()); };

			Assert.Throws<FileNotFoundException>(action);
		}
	}
}
