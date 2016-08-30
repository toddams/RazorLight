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


		}
	}
}
