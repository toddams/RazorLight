using Microsoft.Extensions.Primitives;
using RazorLight.Compilation;
using RazorLight.Generation;
using RazorLight.Razor;
using System;
using System.IO;
using System.Reflection;

namespace RazorLight.Caching
{
	public class FileSystemCachingProvider : ICachingProvider, IPrecompileCallback
	{
		private readonly MemoryCachingProvider m_cache = new MemoryCachingProvider();
		private readonly string m_baseDir;
		private readonly string m_cacheDir;
		private readonly IFileSystemCachingStrategy m_fileSystemCachingStrategy;

		public FileSystemCachingProvider(string baseDir, string cacheDir, IFileSystemCachingStrategy fileSystemCachingStrategy)
		{
			m_baseDir = baseDir;
			m_cacheDir = cacheDir;
			m_fileSystemCachingStrategy = fileSystemCachingStrategy;
		}

		public string GetAssemblyFilePath(string key, string templateFilePath) => m_fileSystemCachingStrategy.GetCachedFileInfo(key, templateFilePath, m_cacheDir).AssemblyFilePath;

		void IPrecompileCallback.Invoke(IGeneratedRazorTemplate generatedRazorTemplate, byte[] rawAssembly, byte[] rawSymbolStore)
		{
			var srcFilePath = Path.Combine(m_baseDir, generatedRazorTemplate.TemplateKey.Substring(1));
			var (_, asmFilePath, pdbFilePath) = m_fileSystemCachingStrategy.GetCachedFileInfo(generatedRazorTemplate.TemplateKey, srcFilePath, m_cacheDir);
			Directory.CreateDirectory(Path.GetDirectoryName(asmFilePath));
			File.WriteAllBytes(asmFilePath, rawAssembly);
			if (rawSymbolStore != null)
			{
				File.WriteAllBytes(pdbFilePath, rawSymbolStore);
			}
		}

		public void CacheTemplate(string key, Func<ITemplatePage> pageFactory, IChangeToken expirationToken)
		{
			m_cache.CacheTemplate(key, pageFactory, expirationToken);
		}

		public bool Contains(string key) => m_fileSystemCachingStrategy.GetCachedFileInfo(key, Path.Combine(m_baseDir, key), m_cacheDir).UpToDate;

		public void Remove(string key)
		{
			var srcFilePath = Path.Combine(m_baseDir, key);
			var (_, asmFilePath, pdbFilePath) = m_fileSystemCachingStrategy.GetCachedFileInfo(key, srcFilePath, m_cacheDir);
			File.Delete(asmFilePath);
			File.Delete(pdbFilePath);
		}

		public TemplateCacheLookupResult RetrieveTemplate(string key)
		{
			var res = m_cache.RetrieveTemplate(key);
			if (res.Success)
			{
				return res;
			}

			var srcFilePath = Path.Combine(m_baseDir, key);
			var (upToDate, asmFilePath, pdbFilePath) = m_fileSystemCachingStrategy.GetCachedFileInfo(key, srcFilePath, m_cacheDir);
			if (upToDate)
			{
				var rawAssembly = File.ReadAllBytes(asmFilePath);
				var rawSymbolStore = File.Exists(pdbFilePath) ? File.ReadAllBytes(pdbFilePath) : null;
				return new TemplateCacheLookupResult(new TemplateCacheItem(key, CreateTemplatePage));

				ITemplatePage CreateTemplatePage()
				{
					var templatePageType = GetTemplatePageType(rawAssembly, rawSymbolStore);
					m_cache.CacheTemplate(key, CreateTemplatePage2);
					return CreateTemplatePage2();

					ITemplatePage CreateTemplatePage2() => NewTemplatePage(templatePageType);
				}
			}
			return new TemplateCacheLookupResult();
		}

		public static Type GetTemplatePageType(string asmFilePath)
		{
			var rawAssembly = File.ReadAllBytes(asmFilePath);
			var pdbFilePath = asmFilePath.Replace(".dll", ".pdb");
			var rawSymbolStore = File.Exists(pdbFilePath) ? File.ReadAllBytes(pdbFilePath) : null;
			return GetTemplatePageType(rawAssembly, rawSymbolStore);
		}

		public static ITemplatePage NewTemplatePage(Type templatePageType) => (ITemplatePage)Activator.CreateInstance(templatePageType);

		public static Type GetTemplatePageType(byte[] rawAssembly, byte[] rawSymbolStore) => Assembly
			.Load(rawAssembly, rawSymbolStore)
			.GetCustomAttribute<RazorLightTemplateAttribute>()
			.TemplateType;
	}
}
