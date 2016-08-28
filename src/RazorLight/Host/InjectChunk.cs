using Microsoft.AspNetCore.Razor.Chunks;

namespace RazorLight.Host
{
	public class InjectChunk : Chunk
	{
		/// <summary>
		/// Represents the chunk for an @inject statement.
		/// </summary>
		/// <param name="typeName">The type name of the property to be injected</param>
		/// <param name="propertyName">The member name of the property to be injected.</param>
		public InjectChunk(
			string typeName,
			string propertyName)
		{
			TypeName = typeName;
			MemberName = propertyName;
		}

		/// <summary>
		/// Gets or sets the type name of the property to be injected.
		/// </summary>
		public string TypeName { get; set; }

		/// <summary>
		/// Gets or sets the name of the property to be injected.
		/// </summary>
		public string MemberName { get; set; }
	}
}
