using System;
using System.IO;
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
            var engine = new RazorLightEngineBuilder()
                .UseMemoryCachingProvider()
                .UseFileSystemProject(@"C:\Projects\RazorLight\sandbox\RazorLight.Sandbox\Views\Subfolder")
                .Build();

            var page = await engine.CompileTemplateAsync("A");


            using (var writer = new StringWriter())
            {
                await engine.RenderTemplateAsync(page, null, null, writer);

                string result = writer.ToString();
                await writer.FlushAsync();
                Console.WriteLine(result);
            }
        }
    }
}