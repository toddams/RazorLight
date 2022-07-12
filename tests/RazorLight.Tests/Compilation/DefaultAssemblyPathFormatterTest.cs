using RazorLight.Compilation;
using System;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace RazorLight.Tests.Compilation
{
	public class DefaultAssemblyPathFormatterTest
	{
		private readonly ITestOutputHelper _testOutputHelper;

		public DefaultAssemblyPathFormatterTest(ITestOutputHelper testOutputHelper)
		{
			_testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
		}

		[SkippableFact]
		public void Ensure_GetAssemblyPath_Works()
		{
			var assembly = typeof(DefaultAssemblyPathFormatterTest).Assembly;
			_testOutputHelper.WriteLine(assembly.Location);
			var directory = new DefaultAssemblyPathFormatter().GetAssemblyPath(assembly);
			Assert.NotNull(directory);
		}

		[SkippableFact]
		public void Ensure_GetAssemblyPath_MatchesLegacy()
		{
			var assembly = typeof(DefaultAssemblyPathFormatterTest).Assembly;
			_testOutputHelper.WriteLine(assembly.Location);
			var directory = new DefaultAssemblyPathFormatter().GetAssemblyPath(assembly);
			var legacyDir = new LegacyFixAssemblyPathFormatter().GetAssemblyPath(assembly);
			Assert.NotNull(directory);
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// On Windows, legacy formatter returns forward-slash as separator due to UriBuilder.
				// So "normalise" the default one for comparison.
				directory = directory.Replace('\\', '/');
			}

			Assert.Equal(legacyDir, directory);
		}
	}
}
