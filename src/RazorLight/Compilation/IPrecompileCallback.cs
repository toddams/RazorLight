using RazorLight.Generation;

namespace RazorLight.Compilation
{
	public interface IPrecompileCallback
	{
		void Invoke(IGeneratedRazorTemplate generatedRazorTemplate, byte[] rawAssembly, byte[] rawSymbolStore);
	}
}
