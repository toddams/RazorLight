using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;
using Moq;
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
            Assert.Throws<ArgumentNullException>(() => { new DefaultMetadataReferenceManager(null); });
        }

        [Fact]
        public void Ensure_AdditionalMetadata_IsApplied()
        {
            var metadata = new HashSet<MetadataReference>();

            var manager = new DefaultMetadataReferenceManager(metadata);

            Assert.NotNull(manager.AdditionalMetadataReferences);
            Assert.Equal(metadata, manager.AdditionalMetadataReferences);
        }

        [Fact]
        public void Throws_WhenCantResolve_DependencyLibraries()
        {
            var manager = new DefaultMetadataReferenceManager();

            IReadOnlyList<CompilationLibrary> a = new List<CompilationLibrary>();

            Shim classPropShim = Shim.Replace(() => Is.A<DependencyContext>().CompileLibraries).With((DependencyContext @this) => a);

            Exception ex = null;
            try
            {
                PoseContext.Isolate(() => 
                {
                    manager.Resolve(DependencyContext.Default);
                }, classPropShim);

            }
            catch (Exception e)
            {
                ex = e.InnerException;
            }

            Assert.NotNull(ex);
            Assert.NotNull(ex.Message);
            Assert.Equal(ex.Message, "Can't load metadata reference from the entry assembly. " +
                    "Make sure PreserveCompilationContext is set to true in *.csproj file");
        }
    }
}
