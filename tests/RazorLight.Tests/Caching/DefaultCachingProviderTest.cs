using Moq;
using RazorLight.Caching;
using System;
using Xunit;

namespace RazorLight.Tests.Caching
{
	public class DefaultCachingProviderTest
	{
		[Fact]
		public void Throws_WhenCachingWithEmptyParams()
		{
			var cache = new MemoryCachingProvider();

			Assert.Throws<ArgumentNullException>(() => cache.CacheTemplate("someKey", null));
			Assert.Throws<ArgumentNullException>(() => cache.CacheTemplate(null, GetTestFactory()));
		}

		[Fact]
		public void Throws_OnNullTemplateKey_WhenRetrieve()
		{
			var cache = new MemoryCachingProvider();

			Assert.Throws<ArgumentNullException>(() => cache.RetrieveTemplate(null));
		}

		[Fact]
		public void Ensure_TemplateIsStored()
		{
			var cache = new MemoryCachingProvider();

			string templateKey = "key";
			var factory = GetTestFactory(templateKey);

			cache.CacheTemplate(templateKey, factory);

			var cachedFactory = cache.RetrieveTemplate(templateKey);
			Assert.Equal(factory, cachedFactory.Template.TemplatePageFactory);
		}

		[Fact]
		public void Contains_ReturnsTrue_OnCachedTemplate()
		{
			var cache = new MemoryCachingProvider();
			string templateKey = "key";

			cache.CacheTemplate(templateKey, GetTestFactory(templateKey));

			Assert.True(cache.Contains(templateKey));
		}

		[Fact]
		public void Contains_ReturnsFalse_OnNonCachedTemplate()
		{
			var cache = new MemoryCachingProvider();

			Assert.False(cache.Contains("someKey"));
		}

		[Fact]
		public void Returns_EmptyTemplateCacheResult_OnNonExistingTemplate()
		{
			var cache = new MemoryCachingProvider();

			var templateResult = cache.RetrieveTemplate("someKey");

			Assert.NotNull(templateResult);
			Assert.Null(templateResult.Template.TemplatePageFactory);
			Assert.False(templateResult.Success);
		}

		[Fact]
		public void Respects_DisabledEncoding_On_CachedTemplates()
		{
			string templateKey = "Assets.Embedded.Empty.cshtml";
		
			var engine = new RazorLightEngineBuilder()
				.DisableEncoding()
				.UseMemoryCachingProvider()
				.SetOperatingAssembly(typeof(Root).Assembly)
				.UseEmbeddedResourcesProject(typeof(Root))
				
				.Build();
			var testCompileToCache = engine.CompileTemplateAsync(templateKey).Result;
		
			Assert.True(testCompileToCache.DisableEncoding);
		
			var cachedCompile = engine.CompileTemplateAsync(templateKey).Result;
		
			Assert.True(cachedCompile.DisableEncoding);
		
		}

		private Func<ITemplatePage> GetTestFactory(string key = "key")
		{
			var moq = new Mock<ITemplatePage>();

			moq.SetupProperty(t => t.Key, key);

			return new Func<ITemplatePage>(() => moq.Object);
		}
	}
}
