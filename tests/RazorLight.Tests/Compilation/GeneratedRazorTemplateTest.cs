using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Moq;
using RazorLight.Compilation;
using Xunit;

namespace RazorLight.Tests.Compilation
{
    public class GeneratedRazorTemplateTest
    {
        [Fact]
        public void Ensure_Throws_OnNull_ConstructorParams()
        {
            Action firstParamAction = () => { new GeneratedRazorTemplate(null, new Mock<RazorCSharpDocument>().Object); };
            Action secondParamAction = () => { new GeneratedRazorTemplate(null, new Mock<RazorCSharpDocument>().Object); };

            Assert.Throws<ArgumentNullException>(firstParamAction);
            Assert.Throws<ArgumentNullException>(secondParamAction);
        }


    }
}