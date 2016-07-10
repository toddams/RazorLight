using System.IO;
using Xunit;

namespace RazorLight.Tests
{
	public class ConfigurationOptionsTest
    {
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
			var path = @"C:/FolderThatNotExists4444";

			var config = new ConfigurationOptions();

			Assert.Throws<DirectoryNotFoundException>(() => config.ViewsFolder = path);
		}
	}
}
