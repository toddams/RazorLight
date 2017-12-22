using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using RazorLight.Extensions;
using System;

namespace RazorLight.Tests.Extensions
{
    public class ServiceCollectionExtensionsTest
    {
		private string rootPath = PathUtility.GetViewsPath();

		private IServiceCollection GetServices()
		{
			var services = new ServiceCollection();
			var envMock = new Mock<IHostingEnvironment>();
			envMock.Setup(m => m.ContentRootPath).Returns(rootPath);
			services.AddSingleton<IHostingEnvironment>(envMock.Object);

			return services;
		}

        [Fact]
        public void Throws_On_Null_EngineFactoryProvider()
        {
            var services = GetServices();

            Assert.Throws<ArgumentNullException>(() => { services.AddRazorLight(null); });
        }

        [Fact]
        public void Ensure_FactoryMethod_Is_Called()
        {
            var services = GetServices();
            bool called = false;

            services.AddRazorLight(() => 
            {
                called = true;
                return new RazorLightEngineBuilder().Build();
            });

            var provider = services.BuildServiceProvider();
            var engine = provider.GetService<IRazorLightEngine>();

            Assert.NotNull(engine);
            Assert.True(called);
        }
    }
}
