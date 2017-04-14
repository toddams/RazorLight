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

		public void AddRange(IEnumerable<Action<TemplatePage>> items)
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

		public void Remove(Action<TemplatePage> item)
		{
			items.Remove(item);
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
