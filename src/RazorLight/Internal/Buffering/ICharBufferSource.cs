using System;
using System.Collections.Generic;
using System.Text;

namespace RazorLight.Internal
{
    public interface ICharBufferSource
    {
        char[] Rent(int bufferSize);

        void Return(char[] buffer);
    }
}
