using RazorLight;
using RazorLight.Extensions;

namespace Sandbox
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var model = new TestViewModel()
			{
				Title = "Vasya"
			};

			var engine = EngineFactory.CreatePhysical(@"D:\");
			string result = engine.ParseString("Hello @Model.Title ", model);

			System.Console.WriteLine(result);

		}

		class Test
		{
			public int NAme { get; set; }
		}
	}
}
