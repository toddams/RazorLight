using System;
using System.Diagnostics;
using System.IO;
using RazorLight;

namespace Sandbox
{
	public class Program
	{
		public static void Main(string[] args)
		{
			string root = @"D:\MyProjects\RazorLight\tests\RazorLight.Tests";

			var config = new ConfigurationOptions() { ViewsFolder = Path.Combine(root, "Views") };
			var engine = new RazorLightEngine(config);

			var sw = Stopwatch.StartNew();

			string result = engine.ParseFile<TestViewModel>("Test.cshtml", new TestViewModel());
			sw.Stop();
			//Console.WriteLine(sw.ElapsedMilliseconds);
			//sw.Reset();
			//sw.Start();
			//string result2 = engine.ParseFile<TestViewModel>("Test.cshtml", new TestViewModel());
			//sw.Stop();
			//Console.WriteLine(sw.ElapsedTicks);
		}
	}
}
