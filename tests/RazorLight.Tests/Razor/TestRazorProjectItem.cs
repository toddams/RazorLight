using System.Collections.Generic;
using System.Threading.Tasks;
using RazorLight.Razor;
using System.Linq;

namespace RazorLight.Tests.Razor
{
	public class TestRazorProject : RazorLightProject
	{
		public RazorLightProjectItem Value { get; set; }

		public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
		{
			return Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
		}

		public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
		{
			return Task.FromResult(Value);
		}
	}
}
