using Microsoft.AspNetCore.Razor.Language;

namespace RazorLight.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new RazorLightEngine();

            engine.Parse("go.cshtml");

            
        }
    }
}
