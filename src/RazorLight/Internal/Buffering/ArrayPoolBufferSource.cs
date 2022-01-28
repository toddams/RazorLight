using System.Buffers;

namespace RazorLight.Internal.Buffering
{
	public class ArrayPoolBufferSource : ICharBufferSource
	{
		private readonly ArrayPool<char> _pool;

		public ArrayPoolBufferSource(ArrayPool<char> pool)
		{
			_pool = pool;
		}

		public char[] Rent(int bufferSize) => _pool.Rent(bufferSize);

		public void Return(char[] buffer) => _pool.Return(buffer);
	}
}
