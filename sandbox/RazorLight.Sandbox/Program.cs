using System;
using System.Threading.Tasks;

namespace RazorLight.Sandbox
{
    class Program
    {
        static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            var engine = new EngineFactory().ForEmbeddedResources(typeof(Program));

            var model = new
            {
                Name1 = "qwe"
            };

            string s = await engine.CompileRenderAsync("go", model, model.GetType(), null);

            Console.WriteLine(s);
            Console.ReadKey();
        }
    }
}
