using System.IO;
using System.Text;

namespace RazorLight.Razor
{
    public class TextSourceProjectItem : RazorLightProjectItem
    {
        private readonly string _content;
        private readonly string _key;

        public TextSourceProjectItem(string key, string source)
        {
            if (key == null)
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            _key = key;
            _content = source;
        }

        public override string Key => _key;

        public override bool Exists => true;

        public override Stream Read()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_content));
        }
    }
}
