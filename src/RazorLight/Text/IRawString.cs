namespace RazorLight.Text
{
	/// <summary>
	/// Defines the required contract for implementing an encoded string.
	/// </summary>
	public interface IRawString
	{
		/// <summary>
		/// Gets the encoded string.
		/// </summary>
		/// <returns>The encoded string.</returns>
		string ToEncodedString();
	}
}
