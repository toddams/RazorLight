using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace RazorLight.Tests.Extensions
{
	public class RazorLightExtensionsTest
	{
		[Fact]
		public void Ensure_Content_Added_To_DynamicTemplates()
		{
			var options = new RazorLightOptions();
			var engineMock = new Mock<IRazorLightEngine>();

			engineMock.Setup(e => (e.CompileRenderAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Type>(), null))).Returns(Task.FromResult("test"));
			engineMock.SetupGet(e => e.Options).Returns(options);

			string key = "key";
			string content = "content";

			string result = RazorLightExtensions.CompileRenderAsync(engineMock.Object, key, content, new Object(), typeof(Object)).Result;

			Assert.NotEmpty(options.DynamicTemplates);
			Assert.Contains(options.DynamicTemplates, t => t.Key == key && t.Value == content);
		}
	}
}
