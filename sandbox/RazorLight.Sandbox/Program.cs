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
            var engine = new EngineFactory().ForFileSystem(@"C:\Projects\RazorLight\sandbox\RazorLight.Sandbox");
        }
    }
}
