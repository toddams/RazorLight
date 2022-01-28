namespace RazorLight.Internal.Buffering
{
	public interface ICharBufferSource
	{
		char[] Rent(int bufferSize);

		void Return(char[] buffer);
	}
}
