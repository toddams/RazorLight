using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
