using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace RazorLight.Templating
{
	public class PageFactoryResult
	{
		public PageFactoryResult(Func<TemplatePage> pageFactory, IList<IChangeToken> expirationTokens)
		{
			this.PageFactory = pageFactory;
			this.ExpirationTokens = expirationTokens;
		}

		public Func<TemplatePage> PageFactory { get; }

		public IList<IChangeToken> ExpirationTokens { get; }

		public bool Success => PageFactory != null;
	}
}
