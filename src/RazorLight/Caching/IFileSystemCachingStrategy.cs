namespace RazorLight.Caching
{
	public struct CachedFileInfo
	{
		public readonly bool UpToDate;
		public readonly string AssemblyFilePath;
		public readonly string PdbFilePath;

		public CachedFileInfo(bool upToDate, string assemblyFilePath, string pdbFilePath)
		{
			UpToDate = upToDate;
			AssemblyFilePath = assemblyFilePath;
			PdbFilePath = pdbFilePath;
		}

		public void Deconstruct(out bool upToDate, out string assemblyFilePath, out string pdbFilePath)
		{
			upToDate = UpToDate;
			assemblyFilePath = AssemblyFilePath;
			pdbFilePath = PdbFilePath;
		}
	}

	public interface IFileSystemCachingStrategy
	{
		string Name { get; }
		CachedFileInfo GetCachedFileInfo(string key, string templateFilePath, string cacheDir);
	}
}
