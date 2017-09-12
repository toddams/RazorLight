using System.IO;

namespace RazorLight.Razor
{
    public class FileSystemRazorProjectItem : RazorLightProjectItem
    {
        public FileSystemRazorProjectItem(string templateKey, FileInfo fileInfo)
        {
            Key = templateKey;
            File = fileInfo;
        }

        public FileInfo File { get; }

        public override string Key { get; set; }

        public override bool Exists => File.Exists;

        public override string TemplateKey => throw new System.NotImplementedException();

        public override bool TemplateExists => throw new System.NotImplementedException();

        public override Stream Read() => File.OpenRead();
    }
}
