using System;
using RazorLight;

namespace Sandbox
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var engine = new RazorLightEngine2(new ConfigurationOptions()
			{
				ViewsFolder = @"D:\MyProjects\RazorLight\sandbox\Sandbox\Views"
			});

			string result = engine.Go("Test.cshtml", new TestViewModel());

			Console.WriteLine(result);
		}
	}
}
