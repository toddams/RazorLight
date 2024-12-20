using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Text;

namespace RazorLight.Precompile.Tests
{
	internal static class Helper
	{
		public static string RunCommandTrimNewline(params string[] args)
		{
			var sb = RunCommand(args);
			sb.Replace(Environment.NewLine, "");
			return sb.ToString();
		}

		public static StringBuilder RunCommand(params string[] args)
		{
			var sw = new StringWriter();
			Program.ConsoleOut = sw;
			var exitCode = Program.DoRun(args);
			ClassicAssert.Zero(exitCode);
			sw.Close();

			return sw.GetStringBuilder();
		}
	}
}
