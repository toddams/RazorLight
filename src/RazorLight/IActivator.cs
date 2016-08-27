using System;

namespace RazorLight
{
	public interface IActivator
	{
		object CreateInstance(Type type);
	}
}
