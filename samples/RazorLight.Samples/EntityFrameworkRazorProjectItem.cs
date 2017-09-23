using RazorLight.Razor;
using System.IO;
using System.Text;

namespace Samples.EntityFrameworkProject
{
    public class EntityFrameworkRazorProjectItem : RazorLightProjectItem
    {
        private string _content;

        public EntityFrameworkRazorProjectItem(string key, string content)
        {
            Key = key;
            _content = content;
        }

        public override string Key { get; set; }

        public override bool Exists => _content != null;

        public override Stream Read()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_content));
        }
    }
}
