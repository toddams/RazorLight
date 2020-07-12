using System.IO;
using System.Reflection;

namespace RazorLight
{
	public class RazorLightEngineWithFileSystemProjectFactory : IRazorLightEngineFactory
	{
		/// <summary>
		/// Creates a RazorLightEngine instance using FileSystemProject.
		/// </summary>
		/// <param name="operatingAssembly">By default, uses Assembly.GetCallingAssembly method.  If the method that calls GetCallingAssembly method is expanded inline
		/// by the just-in-time (JIT) compiler, or if its caller is expanded inline, the assembly that is returned by GetCallingAssembly may differ unexpectedly.
		/// To protect against unexpected behavior, it is best practice to pass in an explicit Assembly via operatingAssembly.</param>
		/// <param name="fileSystemProjectRoot">By default, uses Directory.GetCurrentDirectory method.</param>
		public IRazorLightEngine Create(Assembly operatingAssembly = null, string fileSystemProjectRoot = null)
		{
			return new RazorLightEngineBuilder()
				// NOTE: GetCallingAssembly isn't supported on WinRT.
				.SetOperatingAssembly(operatingAssembly ?? Assembly.GetCallingAssembly())
				.UseFileSystemProject(fileSystemProjectRoot ?? Directory.GetCurrentDirectory())
				.UseMemoryCachingProvider()
				.Build();
		}
	}
}
