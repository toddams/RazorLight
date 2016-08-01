using System;
using RazorLight.Abstractions;
using RazorLight.Compilation;

namespace RazorLight.Templating
{
    public class DefaultPageFactoryProvider : IPageFactoryProvider
	{
		private readonly Func<string, CompilationResult> _compileDelegate;
		private readonly ICompilerCache _compilerCache;

		public DefaultPageFactoryProvider(Func<string, CompilationResult> compileDelegate, ICompilerCache compilerCache)
		{
			_compileDelegate = compileDelegate;
			_compilerCache = compilerCache;
		}

	    public RazorPageFactoryResult CreateFactory(string relativePath)
	    {
			if (relativePath == null)
			{
				throw new ArgumentNullException(nameof(relativePath));
			}

			if (relativePath.StartsWith("~/", StringComparison.Ordinal))
			{
				// For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
				relativePath = relativePath.Substring(1);
			}

			CompilerCacheResult result = _compilerCache.GetOrAdd(relativePath, _compileDelegate);
			if (result.Success)
			{
				return new RazorPageFactoryResult(result.PageFactory, result.ExpirationTokens);
			}
			else
			{
				return new RazorPageFactoryResult(result.ExpirationTokens);
			}
		}
    }
}
