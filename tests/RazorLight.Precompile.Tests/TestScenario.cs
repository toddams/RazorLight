using RazorLight.Caching;

namespace RazorLight.Precompile.Tests
{
	public record TestScenario
	(
		string Name,
		IFileSystemCachingStrategy ExpectedCachingStrategy,
		Func<string, string?> GetExpectedCacheDirectory,
		Func<string, string> GetExpectedPrecompiledFilePath,
		Func<string, string> GetTemplateKey,
		string[] ExtraCommandLineArgs,
		Action Cleanup
	);
}