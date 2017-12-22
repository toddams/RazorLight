using System;
using Microsoft.AspNetCore.Razor.Language;
using Moq;
using RazorLight.Compilation;
using RazorLight.Generation;
using RazorLight.Razor;
using Xunit;

namespace RazorLight.Tests.Compilation
{
	public class GeneratedRazorTemplateTest
	{
		[Fact]
		public void Ensure_Throws_OnNull_ConstructorParams()
		{
			Action firstParamAction = () => { new GeneratedRazorTemplate(null, new Mock<RazorCSharpDocument>().Object); };
			Action secondParamAction = () => { new GeneratedRazorTemplate(null, new Mock<RazorCSharpDocument>().Object); };

			Assert.Throws<ArgumentNullException>(firstParamAction);
			Assert.Throws<ArgumentNullException>(secondParamAction);
		}

		[Fact]
		public void Ensure_ConstructorParameters_AreApplied()
		{
			var projectItem = new EmbeddedRazorProjectItem(typeof(Root), "ewffw");
			var document = new Mock<RazorCSharpDocument>().Object;

			var template = new GeneratedRazorTemplate(projectItem, document);

			Assert.Same(projectItem, template.ProjectItem);
			Assert.Same(document, template.CSharpDocument);
		}

		[Fact]
		public void TemplateKey_IsProjectKey()
		{
			var projectItem = new EmbeddedRazorProjectItem(typeof(Root), "ewffw");
			var document = new Mock<RazorCSharpDocument>().Object;

			var template = new GeneratedRazorTemplate(projectItem, document);

			Assert.NotNull(template.TemplateKey);
			Assert.Equal(template.TemplateKey, projectItem.Key);
		}

		[Fact]
		public void GeneratedCode_Taken_From_CSharpDocument()
		{
			var projectItem = new EmbeddedRazorProjectItem(typeof(Root), "ewffw");

			string value = "test";
			var moq = new Mock<RazorCSharpDocument>();
			moq.Setup(t => t.GeneratedCode).Returns(value);

			var document = moq.Object;

			var template = new GeneratedRazorTemplate(projectItem, document);

			Assert.NotNull(template.GeneratedCode);
			Assert.Equal(template.GeneratedCode, document.GeneratedCode);
		}
	}
}