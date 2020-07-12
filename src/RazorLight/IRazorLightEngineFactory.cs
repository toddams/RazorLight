using System.Reflection;

namespace RazorLight
{
	public interface IRazorLightEngineFactory
	{
		IRazorLightEngine Create(Assembly operatingAssembly = null, string fileSystemProjectRoot = null);
	}
}
