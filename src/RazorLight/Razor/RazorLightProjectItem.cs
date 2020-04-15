using Microsoft.Extensions.Primitives;
using System.IO;

namespace RazorLight.Razor
{
	public abstract class RazorLightProjectItem
	{
		public IChangeToken ExpirationToken { get; set; }

		/// <summary>
		/// Unique key of the template that was searched
		/// </summary>
		public abstract string Key { get; }

		/// <summary>
		/// Gets if template exists
		/// </summary>
		public abstract bool Exists { get; }


		/// <summary>
		/// Returns 
		/// </summary>
		/// <returns></returns>
		public abstract Stream Read();
	}
}
