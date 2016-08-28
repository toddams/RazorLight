using System;
using System.Collections;
using System.Collections.Generic;

namespace RazorLight.Internal
{
	public class PreRenderActionList : IEnumerable<Action<TemplatePage>>
	{
		private readonly List<Action<TemplatePage>> items;
		private readonly object _lock = new object();

		public PreRenderActionList()
		{
			this.items = new List<Action<TemplatePage>>();
		}

		public void Add(Action<TemplatePage> item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			lock (_lock)
			{
				this.items.Add(item);
			}
		}


		public IEnumerator<Action<TemplatePage>> GetEnumerator()
		{
			lock (_lock)
			{
				return new SafeEnumerator<Action<TemplatePage>>(items.GetEnumerator(), _lock);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
