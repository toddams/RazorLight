using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace RazorLight.Templating
{
    public class PageFactoryResult
    {
	    public Func<TemplatePage> PageFactory { get; set; }

		public IList<IChangeToken> ExpirationTokens { get; set; }

		public bool Success => PageFactory != null;
	}
}
