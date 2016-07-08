using System;
using System.IO;
using RazorLight;

namespace Sandbox
{
	public class Program
    {
        public static void Main(string[] args)
        {
			string appRoot = @"D:\MyProjects\RazorLight\sandbox\Sandbox";

			string view = File.ReadAllText(Path.Combine(appRoot, "Views/Test.cshtml"));

			var engine = new RazorLightEngine();

			string output = engine.ParseString<TestViewModel>(view, new TestViewModel());

			Console.WriteLine(output);
		}
	}
}
