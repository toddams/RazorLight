using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RazorLight.Razor
{
	public class EmptyRazorProject : RazorLightProject
	{
		private static readonly string _message = "Can not resolve a content for the template \"{0}\" as there is no project set." +
			"You can only render a template by passing it's content directly via string using coresponding function overload";

		public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
		{
			throw new NotImplementedException(_message);
		}

		public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
		{
			throw new NotImplementedException(_message);
		}
	}
}
