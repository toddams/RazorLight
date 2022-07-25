using RazorLight.Razor;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RazorLight.Caching
{
	public class FileHashCachingStrategy : IFileSystemCachingStrategy
	{
		public static readonly IFileSystemCachingStrategy Instance = new FileHashCachingStrategy();

		public string Name => "FileHash";

		private static string GetFileHash(string key, string filePath)
		{
			using (var md5Hash = MD5.Create())
			{
				// Byte array representation of source string
				var sourceBytes = File.ReadAllBytes(filePath);
				var keyBytes = Encoding.UTF8.GetBytes(FileSystemRazorProjectHelper.NormalizeKey(key));

				var finalBytes = new byte[sourceBytes.Length + keyBytes.Length];
				sourceBytes.CopyTo(finalBytes, 0);
				keyBytes.CopyTo(finalBytes, sourceBytes.Length);

				// Generate hash value(Byte Array) for input data
				var hashBytes = md5Hash.ComputeHash(finalBytes);

				// Convert hash byte array to string
				var sb = new StringBuilder(hashBytes.Length * 2);
				foreach (byte v in hashBytes)
				{
					sb.Append(v.ToString("x2"));
				}
				return sb.ToString();
			}
		}

		public CachedFileInfo GetCachedFileInfo(string key, string templateFilePath, string cacheDir)
		{
			var srcFileHash = GetFileHash(key, templateFilePath);
			var asmFilePath = Path.Combine(cacheDir, srcFileHash + ".dll");
			var pdbFilePath = Path.Combine(cacheDir, srcFileHash + ".pdb");
			return new CachedFileInfo(File.Exists(asmFilePath), asmFilePath, pdbFilePath);
		}
	}
}
