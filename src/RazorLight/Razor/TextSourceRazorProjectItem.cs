using System;
using System.IO;
using System.Text;

namespace RazorLight.Razor
{
	public class TextSourceRazorProjectItem : RazorLightProjectItem
	{
		private string _content;

		public TextSourceRazorProjectItem(string key, string content)
		{
			Key = key ?? throw new ArgumentNullException(nameof(key));

			_content = content ?? throw new ArgumentNullException(nameof(content));
		}

		public override string Key { get; }

		public override bool Exists => true;

		public string Content => _content;

		public override Stream Read()
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(_content));
		}
	}
}
