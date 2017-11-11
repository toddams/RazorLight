using Microsoft.AspNetCore.Razor.Language;
using RazorLight.Razor;
using Xunit;
using System.Linq;
using System.Collections.Generic;

namespace RazorLight.Tests
{
    public class RazorSourceGeneratorTest
    {
        private RazorSourceGenerator NewGenerator()
        {
            return new RazorSourceGenerator(
                RazorEngine.Create(),
                new EmbeddedRazorProject(typeof(RazorSourceGeneratorTest)));
        }

        [Fact]
        public void Ensure_Engine_And_Project_Not_Null()
        {
            var generator = NewGenerator();

            Assert.NotNull(generator.Engine);
            Assert.NotNull(generator.Project);
        }

        [Fact]
        public void Ensure_Default_Imports_Are_Applied()
        {
            var generator = NewGenerator();
            var defaultImports = generator.GetDefaultImportLines().ToList();

            Assert.NotEmpty(defaultImports);

            Assert.Contains(defaultImports, i => i == "@using System");
            Assert.Contains(defaultImports, i => i == "@using System.Threading.Tasks");
            Assert.Contains(defaultImports, i => i == "@using System.Collections.Generic");
            Assert.Contains(defaultImports, i => i == "@using System.Linq");
        }

		//TODO: add tests for imports, etc
		[Fact]
		public void Ensure_Namespaces_Imports_Are_Assigned()
		{
			var namespaces = new HashSet<string>
			{
				"System.Diagnostics",
				"System.CodeDom"
			};

			var generator = new RazorSourceGenerator(RazorEngine.Create(), new EmbeddedRazorProject(typeof(Root)), namespaces);

			Assert.NotNull(generator.Namespaces);
			Assert.Equal(generator.Namespaces, namespaces);
		}
    }
}
