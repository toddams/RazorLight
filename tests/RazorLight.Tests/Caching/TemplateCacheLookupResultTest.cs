using Moq;
using RazorLight.Caching;
using Xunit;

namespace RazorLight.Tests.Caching
{
    public class TemplateCacheLookupResultTest
    {
        [Fact]
        public void EmptyConstructor_EqualsFailedResult()
        {
            var result = new TemplateCacheLookupResult();

            Assert.False(result.Success);
        }

        [Fact]
        public void PassedTemplate_ReturnsSuccessTrue()
        {
            var template = Mock.Of<ITemplatePage>();

            var item = new TemplateCacheItem("key", new System.Func<ITemplatePage>(() => template));

            var result = new TemplateCacheLookupResult(item);

            Assert.True(result.Success);
        }
    }
}
