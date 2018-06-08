using RazorLight.Compilation;
using RazorLight.Razor;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RazorLight.Tests
{
	public class TemplateFactoryProviderTest
    {
		[Fact]
		public void Ensure_FactoryReturnsValidTemplateType()
		{
			string templateKey = "testKey";

			var templateFactoryProvider = new TemplateFactoryProvider();
			var descriptor = new CompiledTemplateDescriptor()
			{
				TemplateAttribute = new RazorLightTemplateAttribute(templateKey, typeof(TestFactoryClass)),
				TemplateKey = templateKey
			};

			Func<ITemplatePage> result = templateFactoryProvider.CreateFactory(descriptor);

			Assert.NotNull(result);
			Assert.IsAssignableFrom<TemplatePage>(result());
		}


		class TestFactoryClass : TemplatePage
		{
			public override Task ExecuteAsync()
			{
				throw new NotImplementedException();
			}

			public override void SetModel(object model)
			{
				throw new NotImplementedException();
			}
		}
	}
}
