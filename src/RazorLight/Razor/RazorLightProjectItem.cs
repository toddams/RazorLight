using System.IO;

namespace RazorLight.Razor
{
    public abstract class RazorLightProjectItem
    {
        public abstract string Key { get; set; }

        public abstract bool Exists { get; }

        public abstract Stream Read();
    }
}
