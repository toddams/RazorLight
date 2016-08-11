using System.IO;

namespace RazorLight.Tests
{
    public class PathUtility
    {
        public static string GetViewsPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Views");
        }
    }
}
