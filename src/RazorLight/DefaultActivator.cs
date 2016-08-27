using System;

namespace RazorLight
{
	public class DefaultActivator : IActivator
	{
		public object CreateInstance(Type type)
		{
			return Activator.CreateInstance(type);
		}
	}
}
