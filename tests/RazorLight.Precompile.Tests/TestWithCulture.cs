using System.Globalization;

namespace RazorLight.Precompile.Tests
{
	public class TestWithCulture
	{
		public TestWithCulture()
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		}
	}
}