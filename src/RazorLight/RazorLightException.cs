using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorLight
{
	public class RazorLightException : Exception
	{
		public RazorLightException()
		{
		}

		public RazorLightException(string message) : base(message) { }

		public RazorLightException(string message, Exception exception) : base(message, exception) { }
    }
}
