using Microsoft.CodeAnalysis;
using RazorLight.Compilation;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace RazorLight.Tests.Compilation
{
	public class DefaultAssemblyDirectoryFormatterTest
	{
		private ITestOutputHelper _testOutputHelper;
		public DefaultAssemblyDirectoryFormatterTest(ITestOutputHelper testOutputHelper)
		{
			_testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
		}

		[SkippableFact]
		public void Ensure_GetAssemblyDirectory_Works_On_Windows()
		{
			// #481 System.UriFormatException: Invalid URI: The hostname could not be parsed
			Skip.If(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX));
			var assembly = typeof(DefaultAssemblyDirectoryFormatterTest).Assembly;
			var formatter = new DefaultAssemblyDirectoryFormatter();
			_testOutputHelper.WriteLine(assembly.Location);
			var directory = formatter.GetAssemblyDirectory(assembly);
			Assert.NotNull(directory);
		}
	}
}
