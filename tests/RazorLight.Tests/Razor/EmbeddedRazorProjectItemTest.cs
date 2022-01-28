using RazorLight.Razor;
using System;
using System.IO;
using Xunit;

namespace RazorLight.Tests.Razor
{
	public class EmbeddedRazorProjectItemTest
	{
		[Fact]
		public void Throws_On_Null_Assembly()
		{
			Assert.Throws<ArgumentNullException>(() => new EmbeddedRazorProjectItem(null, "namespace", "key"));
		}

		[Fact]
		public void Throws_On_Null_Key()
		{
			Assert.Throws<ArgumentNullException>(() => new EmbeddedRazorProjectItem(typeof(Root).Assembly, "namespace", null));
		}

		//[Fact]
		//public void Ensure_ConstructorParams_AreApplied()
		//{
		//    string templateKey = "Assets.Embedded.Empty";
		//    Type rootType = typeof(Root);

		//    var item = new EmbeddedRazorProjectItem(rootType, templateKey);

		//    Assert.Equal(item.Key, templateKey);
		//    Assert.Equal(item.RootType, rootType);
		//    Assert.Equal(item.Assembly, rootType.GetTypeInfo().Assembly);
		//}

		[Fact]
		public void ReturnsExistsTrue_OnExistingTemplate()
		{
			string templateKey = "Assets.Embedded.Empty.cshtml";

			var item = new EmbeddedRazorProjectItem(typeof(Root), templateKey);

			Assert.NotNull(item);
			Assert.True(item.Exists);
		}

		[Fact]
		public void ReturnsExistFalse_OnNonExistingTemplate()
		{
			string templateKey = "Assets.Embedded.IDoNotExist.cshtml";

			var item = new EmbeddedRazorProjectItem(typeof(Root), templateKey);

			Assert.NotNull(item);
			Assert.False(item.Exists);
		}

		[Fact]
		public void Read_ReturnsResourceContent()
		{
			Type rootType = typeof(Root);
			string templateKey = "Assets.Embedded.Empty.cshtml";
			string fullTemplateKey = rootType.Namespace + ".Assets.Embedded.Empty.cshtml";

			string resourceContent = null;
			using (var reader = new StreamReader(rootType.Assembly.GetManifestResourceStream(fullTemplateKey)))
			{
				resourceContent = reader.ReadToEnd();
			}

			var item = new EmbeddedRazorProjectItem(typeof(Root), templateKey);

			string projectContent = null;
			using (var reader = new StreamReader(item.Read()))
			{
				projectContent = reader.ReadToEnd();
			}

			Assert.NotNull(projectContent);
			Assert.Equal(resourceContent, projectContent);
		}
	}
}
