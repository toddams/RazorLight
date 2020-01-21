using System;
using System.IO;

namespace RazorLight.Precompile
{
	internal class TemplateFileInfo
	{
		public TemplateFileInfo(string fullPath, string viewEnginePath)
		{
			FullPath = fullPath;
			ViewEnginePath = viewEnginePath;
		}

		public string FullPath { get; }

		public string ViewEnginePath { get; }

		public Stream CreateReadStream()
		{
			// We are setting buffer size to 1 to prevent FileStream from allocating it's internal buffer
			// 0 causes constructor to throw
			var bufferSize = 1;
			return new FileStream(
				FullPath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.ReadWrite,
				bufferSize,
				FileOptions.Asynchronous | FileOptions.SequentialScan);
		}
	}
}
