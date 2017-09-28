using RazorLight.Razor;
using Xunit;

namespace RazorLight.Tests
{
    public class TemplateFactoryProviderTest
    {
        private TemplateFactoryProvider GetProvider()
        {
            var project = new EmbeddedRazorProject(typeof(TemplateFactoryProviderTest));
            var sourceGenerator = new RazorSourceGenerator(new EngineFactory().RazorEngine, project);
            var compiler = new Compilation.RoslynCompilationService();

            var provider = new TemplateFactoryProvider(sourceGenerator, compiler);

            return provider;
        }

        [Fact]
        public void Throws_On_NullTemplateKey()
        {
            var provider = GetProvider();

            Assert.ThrowsAsync<System.ArgumentNullException>(async () => await provider.CreateFactoryAsync(null));
        }

        [Fact]
        public void Returns_Diagnostics_OnErrors()
        {
            var provider = GetProvider();

            TemplateGenerationException ex = null;

            try
            {
                provider.CreateFactoryAsync("Assets.Embedded.WrongRazorSyntax").GetAwaiter().GetResult();
            }
            catch (TemplateGenerationException exception)
            {
                ex = exception;
            }

            Assert.NotNull(ex);
            Assert.NotEmpty(ex.Diagnostics);
        }

        [Fact]
        public void Ensure_FactoryReturnsValidTemplateType()
        {
            var provider = GetProvider();
            string templateKey = "Assets.Embedded.Empty";

            TemplateFactoryResult result = provider.CreateFactoryAsync(templateKey).GetAwaiter().GetResult();
            var templatePage = result.TemplatePageFactory();

            Assert.True(result.Success);
            Assert.NotNull(result.TemplateDescriptor);
            Assert.NotNull(result.TemplatePageFactory);
            Assert.NotNull(templatePage);

            Assert.Equal(templatePage.Key, templateKey);
            Assert.IsAssignableFrom<TemplatePage>(templatePage);
        }
    }
}
