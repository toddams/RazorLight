using System;

namespace RazorLight.Host.Internal
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class RazorInjectAttribute : Attribute
	{
	}
}
