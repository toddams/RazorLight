using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using RazorLight.Generation;
using RazorLight.Razor;

namespace RazorLight.Compilation
{
	public class RazorTemplateCompiler : IRazorTemplateCompiler
	{
		private readonly SemaphoreSlim  _cacheLock = new SemaphoreSlim(1, 1);

		private RazorSourceGenerator _razorSourceGenerator;
		private ICompilationService _compiler;

		private readonly RazorLightOptions _razorLightOptions;
		private readonly RazorLightProject _razorProject;
		private readonly IMemoryCache _cache;
		private readonly ConcurrentDictionary<string, string> _normalizedKeysCache;
		private readonly Dictionary<string, CompiledTemplateDescriptor> _precompiledViews;

		public RazorTemplateCompiler(
			RazorSourceGenerator sourceGenerator,
			ICompilationService compilationService,
			RazorLightProject razorLightProject,
			RazorLightOptions razorLightOptions)
		{
			_razorSourceGenerator = sourceGenerator ?? throw new ArgumentNullException(nameof(sourceGenerator));
			_compiler = compilationService ?? throw new ArgumentNullException(nameof(compilationService));
			_razorProject = razorLightProject ?? throw new ArgumentNullException(nameof(razorLightProject));
			_razorLightOptions = razorLightOptions ?? throw new ArgumentNullException(nameof(razorLightOptions));

			// This is our L0 cache, and is a durable store. Views migrate into the cache as they are requested
			// from either the set of known precompiled views, or by being compiled.
			var cacheOptions = Options.Create(new MemoryCacheOptions());
			_cache = new MemoryCache(cacheOptions);

			_normalizedKeysCache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

			// We need to validate that the all of the precompiled views are unique by path (case-insensitive).
			// We do this because there's no good way to canonicalize paths on windows, and it will create
			// problems when deploying to linux. Rather than deal with these issues, we just don't support
			// views that differ only by case.
			_precompiledViews = new Dictionary<string, CompiledTemplateDescriptor>(
				5, //Change capacity when precompiled views are arrived
				StringComparer.OrdinalIgnoreCase);
		}

		public RazorTemplateCompiler(
			RazorSourceGenerator sourceGenerator,
			ICompilationService compilationService,
			RazorLightProject razorLightProject,
			IOptions<RazorLightOptions> options) : this(sourceGenerator, compilationService, razorLightProject, options.Value)
		{

		}

		public ICompilationService CompilationService => _compiler;

		internal IMemoryCache Cache => _cache;

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

			string normalizedPath = GetNormalizedKey(templateKey);
			if (_cache.TryGetValue(normalizedPath, out cachedResult))
			{
				return cachedResult;
			}

			// Entry does not exist. Attempt to create one.
			cachedResult = OnCacheMissAsync(templateKey);
			return cachedResult;
		}

		/// <summary>
		/// For testing purposes only.
		/// </summary>
		internal Type ProjectType =>  _razorProject.GetType();

		private async Task<CompiledTemplateDescriptor> OnCacheMissAsync(string templateKey)
		{
			ViewCompilerWorkItem item;
			TaskCompletionSource<CompiledTemplateDescriptor> taskSource;
			MemoryCacheEntryOptions cacheEntryOptions;

			// Safe races cannot be allowed when compiling Razor pages. To ensure only one compilation request succeeds
			// per file, we'll lock the creation of a cache entry. Creating the cache entry should be very quick. The
			// actual work for compiling files happens outside the critical section.
			await _cacheLock.WaitAsync();
			try
			{
				string normalizedKey = GetNormalizedKey(templateKey);

				// Double-checked locking to handle a possible race.
				if (_cache.TryGetValue(normalizedKey, out Task<CompiledTemplateDescriptor> result))
				{
					return await result;
				}

				if (_precompiledViews.TryGetValue(normalizedKey, out var precompiledView))
				{
					item = null;
					// TODO: PrecompiledViews should be generated from RazorLight.Precompile.csproj but it's a work in progress.
					//item = CreatePrecompiledWorkItem(normalizedKey, precompiledView);
				}
				else
				{
					item = await CreateRuntimeCompilationWorkItem(templateKey);
				}

				// At this point, we've decided what to do - but we should create the cache entry and
				// release the lock first.
				cacheEntryOptions = new MemoryCacheEntryOptions();
				if (item.ExpirationToken != null)
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

				_ = _cache.Set(item.NormalizedKey, taskSource.Task, cacheEntryOptions);
			}
			finally
			{
				_cacheLock.Release();
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
					CompiledTemplateDescriptor descriptor = await CompileAndEmitAsync(item.ProjectItem);
					descriptor.ExpirationToken = cacheEntryOptions.ExpirationTokens.FirstOrDefault();
					taskSource.SetResult(descriptor);
				}
				catch (Exception ex)
				{
					taskSource.SetException(ex);
				}
			}

			return await taskSource.Task;
		}

		private async Task<ViewCompilerWorkItem> CreateRuntimeCompilationWorkItem(string templateKey)
		{
			RazorLightProjectItem projectItem;

			if (_razorLightOptions.DynamicTemplates.TryGetValue(templateKey, out string templateContent))
			{
				projectItem = new TextSourceRazorProjectItem(templateKey, templateContent);
			}
			else
			{
				string normalizedKey = GetNormalizedKey(templateKey);
				projectItem = await _razorProject.GetItemAsync(normalizedKey);
			}

			if (!projectItem.Exists)
			{
				var templateNotFoundException = await CreateTemplateNotFoundException(projectItem);
				throw templateNotFoundException;
			}

			return new ViewCompilerWorkItem
			{
				SupportsCompilation = true,

				ProjectItem = projectItem,
				NormalizedKey = projectItem.Key,
				ExpirationToken = projectItem.ExpirationToken,
			};
		}

		protected virtual async Task<CompiledTemplateDescriptor> CompileAndEmitAsync(RazorLightProjectItem projectItem)
		{
			IGeneratedRazorTemplate generatedTemplate = await _razorSourceGenerator.GenerateCodeAsync(projectItem);
			Assembly assembly = _compiler.CompileAndEmit(generatedTemplate);

			// Anything we compile from source will use Razor 2.1 and so should have the new metadata.
			var loader = new RazorCompiledItemLoader();
			var item = loader.LoadItems(assembly).SingleOrDefault();
			var attribute = assembly.GetCustomAttribute<RazorLightTemplateAttribute>();

			return new CompiledTemplateDescriptor
			{
				Item = item,
				TemplateKey = projectItem.Key,
				TemplateAttribute = attribute
			};
		}

		#region helpers

		internal string GetNormalizedKey(string templateKey)
		{
			Debug.Assert(templateKey != null);

			//Support path normalization only on Filesystem projects
			if (!(_razorProject is FileSystemRazorProject))
			{
				return templateKey;
			}

			if (templateKey.Length == 0)
			{
				return templateKey;
			}

			if (_normalizedKeysCache.TryGetValue(templateKey, out var normalizedPath))
				return normalizedPath;

			normalizedPath = _razorProject.NormalizeKey(templateKey);
			_normalizedKeysCache[templateKey] = normalizedPath;

			return normalizedPath;
		}

		internal async Task<TemplateNotFoundException> CreateTemplateNotFoundException(RazorLightProjectItem projectItem)
		{
			var msg = $"{nameof(RazorLightProjectItem)} of type {projectItem.GetType().FullName} with key {projectItem.Key} could not be found by the " +
				$"{ nameof(RazorLightProject)} of type { _razorProject.GetType().FullName} and does not exist in dynamic templates. ";

			var propNames = $"\"{nameof(TemplateNotFoundException.KnownDynamicTemplateKeys)}\" and \"{nameof(TemplateNotFoundException.KnownProjectTemplateKeys)}\"";

			if (_razorLightOptions.EnableDebugMode ?? false)
			{
				msg += $"See the {propNames} properties for known template keys.";

				var dynamicKeys = _razorLightOptions.DynamicTemplates.Keys.ToList();

				var projectKeys = await _razorProject.GetKnownKeysAsync();
				projectKeys = projectKeys?.ToList() ?? Enumerable.Empty<string>();

				return new TemplateNotFoundException(msg, dynamicKeys, projectKeys);
			}
			else
			{
				msg += $"Set {nameof(RazorLightOptions)}.{nameof(RazorLightOptions.EnableDebugMode)} to true to allow " +
					$"the {propNames} properties on this exception to be set.";

				return new TemplateNotFoundException(msg);
			}
		}

		private class ViewCompilerWorkItem
		{
			public bool SupportsCompilation { get; set; }

			public string NormalizedKey { get; set; }

			public IChangeToken ExpirationToken { get; set; }

			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public CompiledTemplateDescriptor Descriptor { get; set; }

			public RazorLightProjectItem ProjectItem { get; set; }
		}

		#endregion
	}
}
