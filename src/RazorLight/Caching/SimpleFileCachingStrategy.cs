using System.IO;

namespace RazorLight.Caching
{
	public class SimpleFileCachingStrategy : IFileSystemCachingStrategy
	{
		public static readonly IFileSystemCachingStrategy Instance = new SimpleFileCachingStrategy();

		public string Name => "Simple";

		public CachedFileInfo GetCachedFileInfo(string key, string templateFilePath, string cacheDir)
		{
			if (key[0] == '/' || key[0] == '\\')
			{
				key = key.Substring(1);
			}
			
			var asmFilePath = Path.Combine(cacheDir, key + ".dll");
			var pdbFilePath = Path.Combine(cacheDir, key + ".pdb");
			var upToDate = false;
			if (File.Exists(asmFilePath))
			{
				var templateFileTime = File.GetLastWriteTimeUtc(templateFilePath);
				var asmFileTime = File.GetLastWriteTimeUtc(asmFilePath);
				upToDate = templateFileTime < asmFileTime;
			}
			return new CachedFileInfo(upToDate, asmFilePath, pdbFilePath);
		}
	}
}
