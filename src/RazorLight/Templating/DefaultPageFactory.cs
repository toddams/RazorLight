using System;
using RazorLight.Compilation;

namespace RazorLight.Templating
{
	public class DefaultPageFactory : IPageFactoryProvider
	{
		private readonly Func<string, CompilationResult> _compileDelegate;

		public DefaultPageFactory(Func<string, CompilationResult> compileDelegate)
		{
			_compileDelegate = compileDelegate;
		}

		public PageFactoryResult CreateFactory(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			CompilationResult result = _compileDelegate(key);
			result.EnsureSuccessful();

			var pageFactory = new Func<TemplatePage>(() => (TemplatePage)Activator.CreateInstance(result.CompiledType));

			return new PageFactoryResult(pageFactory, expirationTokens: null);
		}
	}
}
