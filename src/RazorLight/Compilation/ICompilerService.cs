namespace RazorLight.Compilation
{
	public interface ICompilerService
	{
		CompilationResult Compile(CompilationContext context);
	}
}
