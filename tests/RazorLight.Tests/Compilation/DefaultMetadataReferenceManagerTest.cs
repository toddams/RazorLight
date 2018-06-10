using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;
using RazorLight.Compilation;
using System;
using System.Collections.Generic;
using Xunit;
using Pose;

namespace RazorLight.Tests.Compilation
{
    public class DefaultMetadataReferenceManagerTest
    {
        [Fact]
        public void Throws_OnEmptyManager_InConstructor()
        {
            Assert.Throws<ArgumentNullException>(() => { new DefaultMetadataReferenceManager(null, null); });
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
