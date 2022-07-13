using System.Reflection;

namespace RazorLight.Compilation
{
	public interface IAssemblyPathFormatter
	{
		string GetAssemblyPath(Assembly assembly);
	}
}
