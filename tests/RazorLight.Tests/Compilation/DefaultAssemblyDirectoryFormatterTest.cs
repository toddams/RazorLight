using Microsoft.CodeAnalysis;
using RazorLight.Compilation;
using System;
using System.Collections.Generic;
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

		[Fact]
		public void Ensure_AdditionalMetadata_IsApplied()
		{
			var assembly = typeof(DefaultAssemblyDirectoryFormatterTest).Assembly;
			var formatter = new DefaultAssemblyDirectoryFormatter();
			_testOutputHelper.WriteLine(assembly.Location);
			var directory = formatter.GetAssemblyDirectory(assembly);
			Assert.NotNull(directory);
		}
	}
}
