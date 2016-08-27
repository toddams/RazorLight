using System;
using RazorLight.Caching;
using RazorLight.Compilation;

namespace RazorLight.Templating
{
	public class CachingPageFactory : IPageFactoryProvider
	{
		private readonly Func<string, CompilationResult> _compileDelegate;
		private readonly ICompilerCache _compilerCache;

		public CachingPageFactory(Func<string, CompilationResult> compileDelegate, ICompilerCache compilerCache)
		{
			this._compileDelegate = compileDelegate;
			this._compilerCache = compilerCache;
		}

		public PageFactoryResult CreateFactory(string key)
		{
			if (key.StartsWith("~/", StringComparison.Ordinal))
			{
				// For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
				key = key.Substring(1);
			}

			CompilerCacheResult compilerCacheResult = _compilerCache.GetOrAdd(key, _compileDelegate);

			return new PageFactoryResult(compilerCacheResult.PageFactory, compilerCacheResult.ExpirationTokens);
		}
	}
}
