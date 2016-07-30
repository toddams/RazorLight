using System.IO;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace RazorLight.Tests
{
	public class ConfigurationOptionsTest
	{
		string unexistingDirectory = @"C:/some/path/to/unexisting/Folder/58674963";

		[Fact]
		public void ConfigureOptionsHasDefaultValues()
		{
			var options = ConfigurationOptions.Default;

			Assert.NotNull(options.AdditionalCompilationReferences);
			Assert.Equal(options.AdditionalCompilationReferences.Count, 0);
			Assert.NotNull(options.LoadDependenciesFromEntryAssembly);
		}

		[Fact]
		public void InvalidViewsFolderThrows()
		{
			var config = new ConfigurationOptions();

			Assert.Throws<DirectoryNotFoundException>(() => config.ViewsFolder = unexistingDirectory);
		}

		[Fact]
		public void Get_NullFileProvider_ByDefault()
		{
			var engine = new RazorLightEngine();

			Assert.Equal(engine.Config.ViewsFileProvider.GetType(), typeof(NullFileProvider));
		}
	}
}
