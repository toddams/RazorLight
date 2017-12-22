using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RazorLight.Precompile
{
    internal class PrecompilationApplication : CommandLineApplication
    {
        private Type _callingType;

        public PrecompilationApplication(Type callingType)
        {
            _callingType = callingType;

            Name = "razorlight-precompile";
            FullName = "RazorLight Precompilation Utility";
            Description = "Precompiles RazorLight templates.";
            ShortVersionGetter = GetInformationalVersion;

            HelpOption("-?|-h|--help");

            OnExecute(() =>
            {
                ShowHelp();
                return 2;
            });
        }

        public new int Execute(params string[] args)
        {
            try
            {
                return base.Execute(ExpandResponseFiles(args));
            }
            catch (AggregateException ex) when (ex.InnerException != null)
            {
                Error.WriteLine(ex.InnerException.Message);
                Error.WriteLine(ex.InnerException.StackTrace);
                return 1;
            }
            catch (Exception ex)
            {
                Error.WriteLine(ex.Message);
                Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }

        private static string[] ExpandResponseFiles(string[] args)
        {
            var expandedArgs = new List<string>();
            foreach (var arg in args)
            {
                if (!arg.StartsWith("@", StringComparison.Ordinal))
                {
                    expandedArgs.Add(arg);
                }
                else
                {
                    var fileName = arg.Substring(1);
                    expandedArgs.AddRange(File.ReadLines(fileName));
                }
            }

            return expandedArgs.ToArray();
        }

        private string GetInformationalVersion()
        {
            var assembly = _callingType.GetTypeInfo().Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attribute.InformationalVersion;
        }
    }
}
