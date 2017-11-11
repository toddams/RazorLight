using System;
using System.Threading.Tasks;
using Moq;
using RazorLight.Tests.Models;
using Xunit;

namespace RazorLight.Tests
{
    public class RazorLightEngineTest
    {
		//TODO: add string rendering test

		//[Fact]
		//public void Ensure_Content_Added_To_DynamicTemplates()
		//{
		//	var options = new RazorLightOptions();
		//	var engineMock = new Mock<RazorLightEngine>();

		//	engineMock.Setup(e => (e.CompileRenderAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Type>(), null))).Returns(Task.FromResult("test"));
		//	engineMock.SetupGet(e => e.Options).Returns(options);

		//	string key = "key";
		//	string content = "content";

		//	string result = engineMock.Object.CompileRenderAsync(key:key, content:content, model:new TestViewModel(), modelType: typeof(TestViewModel)).Result;

		//	Assert.NotEmpty(options.DynamicTemplates);
		//	Assert.Contains(options.DynamicTemplates, t => t.Key == key && t.Value == content);
		//}
	}
}
