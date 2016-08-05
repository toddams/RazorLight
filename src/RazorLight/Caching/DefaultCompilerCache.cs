using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using RazorLight.Compilation;

namespace RazorLight.Caching
{
	public class DefaultCompilerCache : ICompilerCache
	{
		private readonly IMemoryCache _cache;

		public DefaultCompilerCache()
		{
			_cache = new MemoryCache(new MemoryCacheOptions() { CompactOnMemoryPressure = false });
		}

		public CompilerCacheResult GetOrAdd(string key, Func<string, CompilationResult> compile)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (compile == null)
			{
				throw new ArgumentNullException(nameof(compile));
			}

			Task<CompilerCacheResult> cacheEntry;
			if (!_cache.TryGetValue(key, out cacheEntry))
			{
				cacheEntry = CreateCacheEntry(key, compile);
			}

			return cacheEntry.GetAwaiter().GetResult();
		}

		private Task<CompilerCacheResult> CreateCacheEntry(
			string key,
			Func<string, CompilationResult> compile)
		{
			MemoryCacheEntryOptions cacheEntryOptions = GetMemoryCacheEntryOptions();

			CompilationResult compilationResult = compile(key);
			compilationResult.EnsureSuccessful();

			var result = new CompilerCacheResult(key, compilationResult, cacheEntryOptions.ExpirationTokens);

			return Task.FromResult(result);
		}

		private MemoryCacheEntryOptions GetMemoryCacheEntryOptions()
		{
			var options = new MemoryCacheEntryOptions();
			options.AddExpirationToken(new CancellationChangeToken(CancellationToken.None));

			return options;
		}
	}
}
