using Microsoft.Extensions.Primitives;
using Mono.Cecil;
using RazorLight.Caching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RazorLight.Precompile
{
	public class PrecompiledCachingProvider : ICachingProvider
	{
		public readonly IReadOnlyDictionary<string, string> Map;
		private readonly MemoryCachingProvider m_cache = new();

		public PrecompiledCachingProvider(IEnumerable<string> precompiledTemplateFilePaths, StreamWriter log)
		{
			var map = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var (templateKey, filePath) in precompiledTemplateFilePaths
				.Select(filePath => (templateKey: GetPrecompiledTemplateKey(filePath, log), filePath))
				.Where(o => o.templateKey != null))
			{
				if (map.TryGetValue(templateKey, out var dupe))
				{
					throw new RazorLightException($"The key {templateKey} is associated with at least two precompiled templates - {dupe} and {filePath}");
				}
				map.Add(templateKey, filePath);
			}
			if (map.Count == 0)
			{
				throw new RazorLightException($"Found no precompiled templates.");
			}
			Map = map;
		}

		private static string GetPrecompiledTemplateKey(string filePath, StreamWriter log)
		{
			try
			{
				using var asmDef = AssemblyDefinition.ReadAssembly(filePath);
				var razorLightAttr = asmDef.CustomAttributes.SingleOrDefault(o => o.AttributeType.FullName == "RazorLight.Razor.RazorLightTemplateAttribute");
				if (razorLightAttr != null)
				{
					string templateKey = (string)razorLightAttr.ConstructorArguments[0].Value;
					log?.WriteLine("GetPrecompiledTemplateKey(\"{0}\") = \"{1}\"", filePath, templateKey);
					return templateKey;
				}
			}
			catch { }
			log?.WriteLine("GetPrecompiledTemplateKey(\"{0}\") = null", filePath);
			return null;
		}

		public void CacheTemplate(string key, Func<ITemplatePage> pageFactory, IChangeToken expirationToken) => throw new NotImplementedException();

		public bool Contains(string key) => Map.ContainsKey(key);

		public void Remove(string key) => throw new NotImplementedException();

		public TemplateCacheLookupResult RetrieveTemplate(string key)
		{
			key = key.Replace('\\', '/');
			if (key[0] != '/')
			{
				key = '/' + key;
			}

			var res = m_cache.RetrieveTemplate(key);
			if (res.Success)
			{
				return res;
			}

			if (Map.TryGetValue(key, out var filePath))
			{
				return new TemplateCacheLookupResult(new TemplateCacheItem(key, CreateTemplatePage));

				ITemplatePage CreateTemplatePage()
				{
					var templatePageType = FileSystemCachingProvider.GetTemplatePageType(filePath);
					m_cache.CacheTemplate(key, CreateTemplatePage2);
					return CreateTemplatePage2();

					ITemplatePage CreateTemplatePage2() => FileSystemCachingProvider.NewTemplatePage(templatePageType);
				}
			}
			throw new RazorLightException($"No precompiled template found for the key {key}");
		}
	}
}
