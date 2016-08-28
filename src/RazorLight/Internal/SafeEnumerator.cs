using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace RazorLight.Internal
{
	public class SafeEnumerator<T> : IEnumerator<T>
	{
		private readonly IEnumerator<T> _inner;

		private readonly object _lock;

		public SafeEnumerator(IEnumerator<T> inner, object @lock)
		{
			_inner = inner;
			_lock = @lock;

			Monitor.Enter(_lock);
		}

		public T Current
		{
			get { return _inner.Current; }
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}

		public void Dispose()
		{
			Monitor.Exit(_lock);
		}

		public bool MoveNext()
		{
			return _inner.MoveNext();
		}

		public void Reset()
		{
			_inner.Reset();
		}
	}
}