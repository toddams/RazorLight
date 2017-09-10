using System;
using System.Threading.Tasks;

namespace RazorLight.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            var engine = new EngineFactory().ForFileSystem("C:\\");

            var model = new
            {
                Name1 = "qwe"
            };

            string s = await engine.CompileRenderAsync("go.cshtml", model, model.GetType(), null);


            Console.WriteLine(s);
            Console.ReadKey();
        }
    }
}
