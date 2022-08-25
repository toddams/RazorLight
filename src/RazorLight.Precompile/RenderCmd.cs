using GlobExpressions;
using ManyConsole;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RazorLight.Caching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RazorLight.Precompile
{
	internal class RenderCmd : ConsoleCommand
	{
		private enum StrategyName
		{
			Simple,
			FileHash
		}

		private static readonly Dictionary<StrategyName, IFileSystemCachingStrategy> s_strategyMap = new()
		{
			[StrategyName.Simple] = SimpleFileCachingStrategy.Instance,
			[StrategyName.FileHash] = FileHashCachingStrategy.Instance
		};

		private string m_path;
		private string m_modelFilePath;
		private string m_jsonQuery;
		private SearchOption m_searchOption = SearchOption.TopDirectoryOnly;
		private string m_key;
		private string m_logFilePath;

		public RenderCmd()
		{
			IsCommand("render", "Renders the given precompiled razor template.");

			HasRequiredOption("p|path=", "A comma separated list of folders and/or files. Plain files must be dlls previously produced by the precompile command. " +
				"The given folders are assumed to contain such dlls. By default the folders are scanned non recursively. Minimatch patterns are supported too.", v => m_path = v);
			HasRequiredOption("m|model=", "The path to a JSON file representing the model object to be rendered against the given template.", v => m_modelFilePath = v);
			HasOption("k|key=", "The key of the template to be used to render the given model. Only required when there are more than one precompiled template.", v => m_key = v);
			HasOption("q|jsonQuery=", "Renders the first item returned by the given JSON query.", v => m_jsonQuery = v);
			HasOption("r|recurse", "Instructs the tool to scan the given folders recursively.", _ => m_searchOption = SearchOption.AllDirectories);
			HasOption("l|log=", "An optional log file path", v => m_logFilePath = v);
		}

		public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
		{
			if (remainingArguments.Length > 0)
			{
				throw new ConsoleHelpAsException("Unrecognized command line arguments - " + string.Join(' ', remainingArguments));
			}
			return base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);
		}

		public override int Run(string[] remainingArguments)
		{
			var o = JsonConvert.DeserializeObject<JToken>(File.ReadAllText(m_modelFilePath));
			if (m_jsonQuery != null)
			{
				o = o.SelectToken(m_jsonQuery);
			}
			var model = JsonModel.New(o);

			using var log = m_logFilePath == null ? null : new StreamWriter(m_logFilePath);
			var cachingProvider = new PrecompiledCachingProvider(YieldFiles(), log);

			if (m_key == null)
			{
				if (cachingProvider.Map.Count > 1)
				{
					throw new RazorLightException($"Found {cachingProvider.Map.Count} precompiled templates and no --key argument was given.");
				}
				m_key = cachingProvider.Map.First().Key;
			}
			else if (m_key[0] != '/')
			{
				m_key = '/' + m_key;
			}

			var engine = new RazorLightEngineBuilder()
				.UseCachingProvider(cachingProvider)
				.Build();

			var templatePage = cachingProvider.RetrieveTemplate(m_key).Template.TemplatePageFactory();
			Program.ConsoleOut.WriteLine(engine.Handler.RenderTemplateAsync(templatePage, model).GetAwaiter().GetResult());
			return 0;
		}

		private IEnumerable<string> YieldFiles()
		{
			if (m_path.Contains(','))
			{
				return m_path.Split(',').SelectMany(DoYieldFiles);
			}

			return DoYieldFiles(m_path);

			IEnumerable<string> DoYieldFiles(string fileOrFolderPath)
			{
				if (fileOrFolderPath.Contains('*') || fileOrFolderPath.Contains('?') || fileOrFolderPath.Contains('['))
				{
					return Glob.Files(Directory.GetCurrentDirectory(), fileOrFolderPath);
				}

				if (File.Exists(fileOrFolderPath))
				{
					if (fileOrFolderPath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
					{
						return new[] { fileOrFolderPath };
					}

					throw new RazorLightException($"{fileOrFolderPath} is not a valid precompiled template assembly.");
				}

				if (Directory.Exists(fileOrFolderPath))
				{
					return Directory.EnumerateFiles(fileOrFolderPath, "*.dll", m_searchOption);
				}

				throw new RazorLightException($"{fileOrFolderPath} is not found.");
			}
		}
	}
}
