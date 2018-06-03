using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using RazorLight.Generation;
using RazorLight.Razor;

namespace RazorLight.Compilation
{
	public class RazorTemplateCompiler
	{
		private readonly object _cacheLock = new object();

		private RazorSourceGenerator _razorSourceGenerator;
		private RoslynCompilationService _compiler;

		private readonly RazorLightProject _razorProject;
		private readonly IMemoryCache _cache;
		private readonly ConcurrentDictionary<string, string> _normalizedPathCache;
		private readonly Dictionary<string, CompiledTemplateDescriptor> _precompiledViews;

		public RazorTemplateCompiler(
			RazorSourceGenerator sourceGenerator,
			RoslynCompilationService roslynCompilationService,
			RazorLightProject razorLightProject)
		{
			_razorSourceGenerator = sourceGenerator;
			_compiler = roslynCompilationService;
			_razorProject = razorLightProject;

			// This is our L0 cache, and is a durable store. Views migrate into the cache as they are requested
			// from either the set of known precompiled views, or by being compiled.
			var cacheOptions = Options.Create(new MemoryCacheOptions());
			_cache = new MemoryCache(cacheOptions);

			_normalizedPathCache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

			// We need to validate that the all of the precompiled views are unique by path (case-insenstive).
			// We do this because there's no good way to canonicalize paths on windows, and it will create
			// problems when deploying to linux. Rather than deal with these issues, we just don't support
			// views that differ only by case.
			_precompiledViews = new Dictionary<string, CompiledTemplateDescriptor>(
				5, //Change capacity when precompiled views are arrived
				StringComparer.OrdinalIgnoreCase);
		}

		public Task<CompiledTemplateDescriptor> CompileAsync(string templateKey)
		{
			if (templateKey == null)
			{
				throw new ArgumentNullException(nameof(templateKey));
			}

			// Attempt to lookup the cache entry using the passed in key. This will succeed if the key is already
			// normalized and a cache entry exists.
			if (_cache.TryGetValue(templateKey, out Task<CompiledTemplateDescriptor> cachedResult))
			{
				return cachedResult;
			}

			string normalizedPath = GetNormalizedPath(templateKey);
			if (_cache.TryGetValue(normalizedPath, out cachedResult))
			{
				return cachedResult;
			}

			// Entry does not exist. Attempt to create one.
			cachedResult = OnCacheMissAsync(normalizedPath);
			return cachedResult;
		}

		private Task<CompiledTemplateDescriptor> OnCacheMissAsync(string normalizedKey)
		{
			ViewCompilerWorkItem item;
			TaskCompletionSource<CompiledTemplateDescriptor> taskSource;
			MemoryCacheEntryOptions cacheEntryOptions;

			// Safe races cannot be allowed when compiling Razor pages. To ensure only one compilation request succeeds
			// per file, we'll lock the creation of a cache entry. Creating the cache entry should be very quick. The
			// actual work for compiling files happens outside the critical section.
			lock (_cacheLock)
			{
				// Double-checked locking to handle a possible race.
				if (_cache.TryGetValue(normalizedKey, out Task<CompiledTemplateDescriptor> result))
				{
					return result;
				}

				if (_precompiledViews.TryGetValue(normalizedKey, out var precompiledView))
				{
					item = null;
					//item = CreatePrecompiledWorkItem(normalizedKey, precompiledView);
				}
				else
				{
					item = CreateRuntimeCompilationWorkItem(normalizedKey).GetAwaiter().GetResult();
				}

				// At this point, we've decided what to do - but we should create the cache entry and
				// release the lock first.
				cacheEntryOptions = new MemoryCacheEntryOptions();
				if(item.ExpirationToken != null)
				{
					cacheEntryOptions.ExpirationTokens.Add(item.ExpirationToken);
				}

				taskSource = new TaskCompletionSource<CompiledTemplateDescriptor>();
				if (item.SupportsCompilation)
				{
					// We'll compile in just a sec, be patient.
				}
				else
				{
					// If we can't compile, we should have already created the descriptor
					Debug.Assert(item.Descriptor != null);
					taskSource.SetResult(item.Descriptor);
				}

				_cache.Set(normalizedKey, taskSource.Task, cacheEntryOptions);
			}

			// Now the lock has been released so we can do more expensive processing.
			if (item.SupportsCompilation)
			{
				Debug.Assert(taskSource != null);

				//if (item.Descriptor?.Item != null &&
				//	ChecksumValidator.IsItemValid(_projectEngine, item.Descriptor.Item))
				//{
				//	// If the item has checksums to validate, we should also have a precompiled view.
				//	Debug.Assert(item.Descriptor != null);

				//	taskSource.SetResult(item.Descriptor);
				//	return taskSource.Task;
				//}

				try
				{
					var descriptor = CompileAndEmit(normalizedKey);
					descriptor.ExpirationToken = cacheEntryOptions.ExpirationTokens.FirstOrDefault();
					taskSource.SetResult(descriptor);
				}
				catch (Exception ex)
				{
					taskSource.SetException(ex);
				}
			}

			return taskSource.Task;
		}

		private async Task<ViewCompilerWorkItem> CreateRuntimeCompilationWorkItem(string normalizedKey)
		{
			RazorLightProjectItem projectItem = await _razorProject.GetItemAsync(normalizedKey);
			if (!projectItem.Exists)
			{
				// If the file doesn't exist, we can't do compilation right now - we still want to cache
				// the fact that we tried. This will allow us to retrigger compilation if the view file
				// is added.
				return new ViewCompilerWorkItem()
				{
					// We don't have enough information to compile
					SupportsCompilation = false,

					Descriptor = new CompiledTemplateDescriptor()
					{
						TemplateKey = normalizedKey,
						ExpirationToken = projectItem.ExpirationToken,
					},

					// We can try again if the file gets created.
					ExpirationToken = projectItem.ExpirationToken,
				};
			}

			return new ViewCompilerWorkItem()
			{
				SupportsCompilation = true,

				NormalizedKey = normalizedKey,
				ExpirationToken = projectItem.ExpirationToken,
			};
		}

		protected virtual CompiledTemplateDescriptor CompileAndEmit(string templateKey)
		{
			IGeneratedRazorTemplate generatedTemplate = _razorSourceGenerator.GenerateCodeAsync(templateKey).GetAwaiter().GetResult();
			Assembly assembly = _compiler.CompileAndEmit(generatedTemplate);

			// Anything we compile from source will use Razor 2.1 and so should have the new metadata.
			var loader = new RazorCompiledItemLoader();
			var item = loader.LoadItems(assembly).SingleOrDefault();
			var attribute = assembly.GetCustomAttribute<RazorLightTemplateAttribute>();

			return new CompiledTemplateDescriptor()
			{
				Item = item,
				TemplateKey = templateKey,
				TemplateAttribute = attribute
			};
		}

		#region helpers

		private string GetNormalizedPath(string relativePath)
		{
			Debug.Assert(relativePath != null);
			if (relativePath.Length == 0)
			{
				return relativePath;
			}

			if (!_normalizedPathCache.TryGetValue(relativePath, out var normalizedPath))
			{
				normalizedPath = NormalizeKey(relativePath);
				_normalizedPathCache[relativePath] = normalizedPath;
			}

			return normalizedPath;
		}

		private string NormalizeKey(string templateKey)
		{
			if(!(_razorProject is FileSystemRazorProject))
			{
				return templateKey;
			}

			var addLeadingSlash = templateKey[0] != '\\' && templateKey[0] != '/';
			var transformSlashes = templateKey.IndexOf('\\') != -1;

			if (!addLeadingSlash && !transformSlashes)
			{
				return templateKey;
			}

			var length = templateKey.Length;
			if (addLeadingSlash)
			{
				length++;
			}

			var builder = new InplaceStringBuilder(length);
			if (addLeadingSlash)
			{
				builder.Append('/');
			}

			for (var i = 0; i < templateKey.Length; i++)
			{
				var ch = templateKey[i];
				if (ch == '\\')
				{
					ch = '/';
				}
				builder.Append(ch);
			}

			return builder.ToString();
		}

		private class ViewCompilerWorkItem
		{
			public bool SupportsCompilation { get; set; }

			public string NormalizedKey { get; set; }

			public IChangeToken ExpirationToken { get; set; }

			public CompiledTemplateDescriptor Descriptor { get; set; }
		}

		#endregion
	}
}
