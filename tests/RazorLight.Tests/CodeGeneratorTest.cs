//using System.IO;
//using Xunit;

//namespace RazorLight.Tests
//{
//	public class CodeGeneratorTest
//    {
//		private string appRootPath = @"D:\MyProjects\RazorLight\tests\RazorLight.Tests";
//	    private string content = "Hello, @Model.Title";
//		private TestViewModel model = new TestViewModel();

//		[Fact]
//	    public void AnonymousObject_Ok()
//	    {
//			var codeGenerator = new RazorLightCodeGenerator(ConfigurationOptions.Default);

//		    var model = new
//		    {
//			    Title = "Test anon"
//		    };

//		    string result = codeGenerator.GenerateCode(new StringReader(content), new ModelTypeInfo(model.GetType()));

//			Assert.NotNull(result);
//	    }

//	    public void RealModel_Ok()
//	    {
//			var codeGenerator = new RazorLightCodeGenerator(ConfigurationOptions.Default);

//		    string result = codeGenerator.GenerateCode(new StringReader(content), new ModelTypeInfo(model.GetType()));

//			Assert.NotNull(result);
//	    }
//	}
//}
