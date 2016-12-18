using System;
using System.Collections;
using System.Collections.Generic;

namespace RazorLight.Internal
{
	public class PreRenderActionList : IEnumerable<Action<ITemplatePage>>
	{
		private readonly List<Action<ITemplatePage>> items;
		private readonly object _lock = new object();

		public PreRenderActionList()
		{
			this.items = new List<Action<ITemplatePage>>();
		}

		public void Add(Action<ITemplatePage> item)
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

		public void AddRange(IEnumerable<Action<ITemplatePage>> items)
		{
			if(items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			lock (_lock)
			{
				this.items.AddRange(items);
			}
		}

		public int Count
		{
			get
			{
				lock (_lock)
				{
					return items.Count;
				}
			}
		}

		public IEnumerator<Action<ITemplatePage>> GetEnumerator()
		{
			lock (_lock)
			{
				return new SafeEnumerator<Action<ITemplatePage>>(items.GetEnumerator(), _lock);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
