using System;
using System.IO;

namespace RazorLight.Razor
{
	public sealed class NoRazorProjectItem : RazorLightProjectItem
	{
		private NoRazorProjectItem()
		{
		}

		private static readonly Lazy<NoRazorProjectItem> EmptyImpl = new Lazy<NoRazorProjectItem>(() => new NoRazorProjectItem());
		public static NoRazorProjectItem Empty => EmptyImpl.Value;

		public override string Key { get; }
		public override bool Exists { get; }

		public override Stream Read()
		{
			throw new NotImplementedException($"{nameof(NoRazorProjectItem)} is only used by string templates.");
		}

		public override bool Equals(object obj)
		{
			var other = obj as NoRazorProjectItem;
			return string.Equals(Key, other?.Key);
		}

		private bool Equals(NoRazorProjectItem other)
		{
			return Key == other?.Key;
		}

		public override int GetHashCode()
		{
			return (Key != null ? Key.GetHashCode() : 0);
		}
	}
}
