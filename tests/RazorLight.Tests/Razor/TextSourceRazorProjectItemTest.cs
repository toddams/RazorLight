using RazorLight.Razor;
using System;
using System.IO;
using Xunit;

namespace RazorLight.Tests.Razor
{
	public class TextSourceRazorProjectItemTest
	{
		[Fact]
		public void Throws_OnNullConstructorParams()
		{
			Assert.Throws<ArgumentNullException>(() => new TextSourceRazorProjectItem(null, "some"));
			Assert.Throws<ArgumentNullException>(() => new TextSourceRazorProjectItem("some", null));
		}

		[Fact]
		public void Ensure_ConstructorParams_Applied()
		{
			string templateKey = "key";
			string content = "content";

			var item = new TextSourceRazorProjectItem(templateKey, content);

			Assert.NotNull(item);
			Assert.Equal(templateKey, item.Key);
			Assert.Equal(content, item.Content);
		}

		[Fact]
		public void Exist_AlwaysReturnsTrue()
		{
			var item = new TextSourceRazorProjectItem("some", "some");

			Assert.True(item.Exists);
		}

		[Fact]
		public void Read_Returns_Content()
		{
			string content = "Test content here";

			var item = new TextSourceRazorProjectItem("key", content);

			string projectContent = null;
			using (var stringWriter = new StreamReader(item.Read()))
			{
				projectContent = stringWriter.ReadToEnd();
			}

			Assert.NotNull(projectContent);
			Assert.Equal(projectContent, content);
		}
	}
}
