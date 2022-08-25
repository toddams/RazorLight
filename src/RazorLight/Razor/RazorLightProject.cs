using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorLight.Razor
{
	public abstract class RazorLightProject
	{
		/// <summary>
		/// Looks up for the template source with a given <paramref name="templateKey"/>
		/// </summary>
		/// <param name="templateKey">Unique template key</param>
		/// <returns></returns>
		public abstract Task<RazorLightProjectItem> GetItemAsync(string templateKey);

		/// <summary>
		/// Looks up for the ViewImports content for the given template
		/// </summary>
		/// <param name="templateKey"></param>
		/// <returns></returns>
		public abstract Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey);

		/// <summary>
		/// Looks up all template keys known by the project
		/// </summary>
		/// <returns></returns>
		public virtual Task<IEnumerable<string>> GetKnownKeysAsync()
		{
			return Task.FromResult(Enumerable.Empty<string>());
		}

		public virtual string NormalizeKey(string templateKey) => templateKey;
	}
}
