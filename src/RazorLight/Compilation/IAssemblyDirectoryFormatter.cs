using System.Reflection;

namespace RazorLight.Compilation
{
	public interface IAssemblyDirectoryFormatter
	{
		string GetAssemblyDirectory(Assembly assembly);
	}
}