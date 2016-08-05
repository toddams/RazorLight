using System;
using RazorLight;

namespace Sandbox
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//var engine = EngineFactory.CreatePhysical(@"D:\MyProjects\RazorLight\sandbox\Sandbox");

			//Console.WriteLine(engine.Parse("Test.cshtml", new TestViewModel()));

			var engine2 = EngineFactory.CreateEmbedded(typeof(TestViewModel));

			string result = engine2.Parse("Test", new TestViewModel());

			Console.WriteLine(result);
		}
	}
}
