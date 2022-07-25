using ManyConsole;
using System;
using System.IO;

namespace RazorLight.Precompile
{
	public class Program
	{
		public static TextWriter ConsoleOut { get; set; } = Console.Out;

		public static int Main(string[] args)
		{
			try
			{
				return DoRun(args);
			}
			catch (Exception exc)
			{
				Console.Error.WriteLine(exc);
				return 1;
			}
		}

		public static int DoRun(string[] args)
		{
			var commands = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
			foreach (var c in commands)
			{
				c.SkipsCommandSummaryBeforeRunning();
			}
			return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
		}
	}
}
