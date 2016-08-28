using RazorLight;
using System.Linq;

namespace Sandbox
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var engine = EngineFactory.CreatePhysical(@"D:\MyProjects\RazorLight\sandbox\Sandbox\Views");
			var model = new TestViewModel();

			try
			{
				engine.PreRenderCallbacks.Add(p => System.Console.WriteLine("wefwef"));
				string result2 = engine.Parse("Test.cshtml", model);

				System.Console.WriteLine(result2);
			}
			catch (System.AggregateException ex)
			{
				var e = ex.InnerException as TemplateCompilationException;

				System.Console.WriteLine(e.CompilationErrors.First());
			}
		}
	}
}
