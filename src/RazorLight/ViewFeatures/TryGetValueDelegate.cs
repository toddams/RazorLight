using System;
using System.Collections.Generic;
using System.Text;

namespace RazorLight.ViewFeatures
{
	public delegate bool TryGetValueDelegate(object dictionary, string key, out object value);
}
