using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace RazorLight.Tests
{
	public class RazorLightEngineBuilderTest
	{
		[Fact]
		public void Throws_On_Null_Project()
		{
			Action action = () => new RazorLightEngineBuilder().UseProject(null);

			Assert.Throws<ArgumentNullException>(action);
		}

		[Fact]
		public void Throws_On_Null_RootType_ForEmdeddedProject()
		{
			Action action = () => new RazorLightEngineBuilder().UseEmbeddedResourcesProject(null);

			Assert.Throws<ArgumentNullException>(action);
		}

		[Fact]
		public void Throws_On_Null_RootPath_ForFilesystemProject()
		{
			Action action = () => new RazorLightEngineBuilder().UseFileSystemProject(null);

			Assert.Throws<DirectoryNotFoundException>(action);
		}


		[Fact]
		public void Throws_On_Null_CachingProvider()
		{
			Action action = () => new RazorLightEngineBuilder().UseCachingProvider(null);

			Assert.Throws<ArgumentNullException>(action);
		}

		[Fact]
		public void Throws_On_Null_Namespaces()
		{
			Action action = () => new RazorLightEngineBuilder().AddDefaultNamespaces(null);

			Assert.Throws<ArgumentNullException>(action);
		}

		[Fact]
		public void Throws_On_Null_AddMetadataReferences()
		{
			Action action = () => new RazorLightEngineBuilder().AddMetadataReferences(null);

			Assert.Throws<ArgumentNullException>(action);
		}

		[Fact]
		public void Throws_On_Null_DynamicTemplates()
		{
			Action action = () => new RazorLightEngineBuilder().AddDynamicTemplates(null);

			Assert.Throws<ArgumentNullException>(action);
		}

		[Fact]
		public void Throws_On_Null_PrerenderCallbacks()
		{
			Action action = () => new RazorLightEngineBuilder().AddPrerenderCallbacks(null);

			Assert.Throws<ArgumentNullException>(action);
		}

		[Fact]
		public void Throws_On_Null_Assembly()
		{
			Action action = () => new RazorLightEngineBuilder().SetOperatingAssembly(null);

			Assert.Throws<ArgumentNullException>(action);
		}

		//[Fact]
		//public void Compiler_OperatingAssembly_IsSetTo_EntryAssembly_If_Not_Specified()
		//{
		//    var engine = new RazorLightEngineBuilder().Build();

		//    Assert.Equal(engine.TemplateFactoryProvider.Compiler.OperatingAssembly, Assembly.GetEntryAssembly());
		//}
	}
}
