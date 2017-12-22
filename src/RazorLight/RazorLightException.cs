using System;
using System.Collections.Generic;
using System.Text;

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
