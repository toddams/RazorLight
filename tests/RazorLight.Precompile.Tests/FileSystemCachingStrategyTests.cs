using NUnit.Framework;
using RazorLight.Caching;
using System.Runtime.InteropServices;

namespace RazorLight.Precompile.Tests
{
	public class FileSystemCachingStrategyTests
	{
		private static readonly object[] s_testCases = new object[]
		{
			FileHashCachingStrategy.Instance,
			SimpleFileCachingStrategy.Instance,
		};

		private static readonly string[] s_firstSepOptionsWindows = { "", "/", "\\" };
		private static readonly string[] s_secondSepOptionsWindows = { "/", "\\" };
		private static readonly string[] s_firstSepOptionsUnix = { "", "/" };
		private static readonly string[] s_secondSepOptionsUnix = { "/" };

		private static readonly IEnumerable<string[]> s_sepCombinations = GetSeparatorCombinations();

		private static IEnumerable<string[]> GetSeparatorCombinations()
		{
			string[] firstSepOptions, secondSepOptions;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				firstSepOptions = s_firstSepOptionsWindows;
				secondSepOptions = s_secondSepOptionsWindows;
			}
			else
			{
				firstSepOptions = s_firstSepOptionsUnix;
				secondSepOptions = s_secondSepOptionsUnix;
			}

			foreach (var s11 in firstSepOptions)
			{
				foreach (var s12 in firstSepOptions)
				{
					foreach (var s21 in secondSepOptions)
					{
						foreach (var s22 in secondSepOptions)
						{
							if (s11 != s12 || s21 != s22)
							{
								yield return new[] { s11, s21, s12, s22 };
							}
						}
					}
				}
			}
		}

		[TestCaseSource(nameof(s_testCases))]
		public void DifferentKey(IFileSystemCachingStrategy s)
		{
			var templateFilePath = "Samples/folder/MessageItem.cshtml";
			var o1 = s.GetCachedFileInfo("folder/MessageItem.cshtml", templateFilePath, "X:/");
			var o2 = s.GetCachedFileInfo("MessageItem.cshtml", templateFilePath, "X:/");
			Assert.AreNotEqual(o1.AssemblyFilePath, o2.AssemblyFilePath);
		}

		[TestCaseSource(nameof(s_sepCombinations))]
		public void EquivalentKeyFileHashCachingStrategy(string[] sepCombination)
		{
			var (asmFilePath1, asmFilePath2) = GetAsmFilePaths(FileHashCachingStrategy.Instance, sepCombination);
			Assert.AreEqual(asmFilePath1, asmFilePath2);
		}

		[TestCaseSource(nameof(s_sepCombinations))]
		public void EquivalentKeySimpleFileCachingStrategy(string[] sepCombination)
		{
			var (asmFilePath1, asmFilePath2) = GetAsmFilePaths(SimpleFileCachingStrategy.Instance, sepCombination);
			if (asmFilePath1 != asmFilePath2)
			{
				asmFilePath1 = Path.GetFullPath(asmFilePath1);
				asmFilePath2 = Path.GetFullPath(asmFilePath2);
			}
			Assert.AreEqual(asmFilePath1, asmFilePath2);
		}

		private static (string, string) GetAsmFilePaths(IFileSystemCachingStrategy s, string[] sepCombination)
		{
			var templateFilePath = "Samples/folder/MessageItem.cshtml";
			string key1 = $"{sepCombination[0]}folder{sepCombination[1]}MessageItem.cshtml";
			string key2 = $"{sepCombination[2]}folder{sepCombination[3]}MessageItem.cshtml";
			Assert.AreNotEqual(key1, key2);
			var asmFilePath1 = s.GetCachedFileInfo(key1, templateFilePath, "X:/").AssemblyFilePath;
			var asmFilePath2 = s.GetCachedFileInfo(key2, templateFilePath, "X:/").AssemblyFilePath;
			return (asmFilePath1, asmFilePath2);
		}
	}
}