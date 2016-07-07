using System;
using RazorLight;

namespace Sandbox
{
	public class Program
    {
        public static void Main(string[] args)
        {
			var engine = new RazorLightEngine();

			var text = System.IO.File.ReadAllText(@"D:\MyProjects\RazorLight\sandbox\Sandbox\Views\Test.cshtml");
			var model = new TestViewModel();

			string result = engine.ParseString<TestViewModel>(text, model);

			Console.WriteLine(result);
        }
    }
}
