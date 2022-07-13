using System.Reflection;

namespace RazorLight.Compilation
{
	public class DefaultAssemblyPathFormatter : IAssemblyPathFormatter
	{
		public string GetAssemblyPath(Assembly assembly) => assembly.Location;
	}
}
