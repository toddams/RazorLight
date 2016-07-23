using System;

namespace RazorLight.Internal
{
	public interface IRazorView
	{
		string Path { get; set; }
	}

    public class LightRazorView : IRazorView
    {
		public string Path { get; set; }
	}
}
