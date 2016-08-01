using System;
using RazorLight;
using RazorLight.Internal;

namespace Sandbox
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var options = new ConfigurationOptions()
			{
				ViewsFolder = @"D:\MyProjects\RazorLight\sandbox\Sandbox\Views"
			};

			var engine = new RazorLightEngine(options);

			var r = engine.ParseFile("Test.cshtml", new TestViewModel());

			var r2 = engine.ParseFile("Test.cshtml", new TestViewModel());

			Console.WriteLine(r);
		}
	}
}
