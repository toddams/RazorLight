using RazorLight.Text;
using System;
using System.IO;
using Xunit;

namespace RazorLight.Tests.Text
{
    public class RawStringTest
    {
        [Fact]
        public void String_Can_Not_Be_Null()
        {
            string value = null;

            Assert.Throws<ArgumentNullException>(() => { new RawString(value); });
        }

        [Fact]
        public void TextWriter_Can_Not_Be_Null()
        {
            TextWriter writer = null;

            var raw = new RawString("value");

            Assert.Throws<ArgumentNullException>(() => { raw.WriteTo(writer); });
        }

        [Fact]
        public void ValueProperty_ReturnsValue()
        {
            string value = "value";

            var raw = new RawString(value);

            Assert.Equal(value, raw.Value);
        }

        [Fact]
        public void WriteTo_Writes_ToSpecifiedWriter()
        {
            string value = "value";
            var raw = new RawString(value);

            var writer = new StringWriter();
            raw.WriteTo(writer);

            string result = writer.ToString();
            Assert.NotNull(result);
            Assert.Equal(value, result);
        }
    }
}
