using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorLight.Abstractions
{
    public interface ICompilerService
    {
	    Type Compile(string content);
    }
}
