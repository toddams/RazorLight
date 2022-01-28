using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RazorLight.Internal
{
	internal class AssemblyDebugModeUtility
	{
		internal static bool IsAssemblyDebugBuild(Assembly assembly)
		{
			return assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled);
		}
	}
}
