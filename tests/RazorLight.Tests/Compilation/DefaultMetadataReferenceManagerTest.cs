using Microsoft.CodeAnalysis;
using RazorLight.Compilation;
using System;
using System.Collections.Generic;
using Xunit;

namespace RazorLight.Tests.Compilation
{
	public class DefaultMetadataReferenceManagerTest
	{
		[Fact]
		public void Throws_OnEmptyManager_InConstructor()
		{
			Assert.Throws<ArgumentNullException>(() => { _ = new DefaultMetadataReferenceManager(null as HashSet<MetadataReference>, null); });
		}

		[Fact]
		public void Ensure_AdditionalMetadata_IsApplied()
		{
			var metadata = new HashSet<MetadataReference>();
			var manager = new DefaultMetadataReferenceManager(metadata);

			Assert.NotNull(manager.AdditionalMetadataReferences);
			Assert.Equal(metadata, manager.AdditionalMetadataReferences);
		}
	}
}
