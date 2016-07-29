using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorLight.Rendering
{
	public class ViewBufferPage
	{
		public ViewBufferPage(ViewBufferValue[] buffer)
		{
			Buffer = buffer;
		}

		public ViewBufferValue[] Buffer { get; }

		public int Capacity => Buffer.Length;

		public int Count { get; set; }

		public bool IsFull => Count == Capacity;

		public void Append(ViewBufferValue value) => Buffer[Count++] = value;
	}
}
