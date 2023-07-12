using System.Reflection;
using Xunit;

namespace RazorLight.Tests.Caching
{
	public class DeletingFromCacheTest
	{
		private readonly RazorLightEngine _engine;
		private readonly TemplateRendererTest.TestModel _model;
		
		public DeletingFromCacheTest()
		{
			_engine = new RazorLightEngineBuilder().SetOperatingAssembly(Assembly.GetExecutingAssembly()).UseMemoryCachingProvider().Build();
			_model = new TemplateRendererTest.TestModel { Value = "TestValue" };
		}

		[Fact]
		public void DeletingOnlyFromHandlerCache_StillTakesTemplateFromCache()
		{
			var testTemplate = "Cached";
			
			Assert.Equal("Cached", _engine.CompileRenderStringAsync("testTemplate", testTemplate, _model).Result);
			Assert.True(_engine.Handler.Cache.Contains("testTemplate"));
			
			_engine.Handler.Cache.Remove("testTemplate");
			
			Assert.False(_engine.Handler.Cache.Contains("testTemplate"));
			
			testTemplate = "Deleted From Cache";
			Assert.NotEqual("Deleted From Cache", _engine.CompileRenderStringAsync("testTemplate", testTemplate, _model).Result);
		}
		
		[Fact]
		public void DeletingFromCompilerCache_FixesCacheProblems()
		{
			var testTemplate = "Cached";
			
			Assert.Equal("Cached", _engine.CompileRenderStringAsync("testTemplate", testTemplate, _model).Result);
			Assert.True(_engine.Handler.Cache.Contains("testTemplate"));
			
			_engine.Handler.Cache.Remove("testTemplate");
			_engine.Handler.Compiler.Cache.Remove("testTemplate");
			
			Assert.False(_engine.Handler.Cache.Contains("testTemplate"));
			Assert.False(_engine.Handler.Compiler.Cache.TryGetValue("testTemplate", out _));
			
			testTemplate = "Deleted From Cache";
			Assert.Equal("Deleted From Cache", _engine.CompileRenderStringAsync("testTemplate", testTemplate, _model).Result);
		}
	}
}