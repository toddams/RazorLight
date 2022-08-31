using ManyConsole;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RazorLight.Caching;
using System.Collections.Generic;
using System.IO;

namespace RazorLight.Precompile
{
	public class PrecompileCmd : ConsoleCommand
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

		private string m_templateFile;
		private string m_cacheDir;
		private string m_baseDir;
		private StrategyName m_strategyName = StrategyName.FileHash;
		private string m_modelFilePath;
		private string m_jsonQuery;

		public PrecompileCmd()
		{
			IsCommand("precompile", "Precompiles the given razor template.");

			HasRequiredOption("t|template=", "The path to a razor template. " +
				"A relative path is based off the current directory or the base directory, if the latter is given.", v => m_templateFile = v);
			HasOption("c|cache=", "The cache directory where precompiled assemblies are stored. Will be created, if does not exist. " +
				"Defaults to the directory containing the template files.", v => m_cacheDir = v);
			HasOption("b|base=", "The razor template base directory. Defaults to the home directory of the given template. " +
				"If given and the template file path is relative, then it is relative to this base directory.", v => m_baseDir = v);
			HasOption("s|strategy=", "The file system caching strategy. The default strategy is " + m_strategyName, (StrategyName v) => m_strategyName = v);
			HasOption("m|model=", "The path to a JSON file representing the model object to be rendered against the given template.", v => m_modelFilePath = v);
			HasOption("q|jsonQuery=", "Renders the first item returned by the given JSON query.", v => m_jsonQuery = v);

			HasLongDescription(
				"Precompiles the given razor template into the given cache directory. " +
				"When the --model argument is given the command also renders the given model and outputs the result to the stdout. " +
				"Otherwise the command outputs the path to the precompiled assembly.");
		}

		public override int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments)
		{
			if (remainingArguments.Length > 0)
			{
				throw new ConsoleHelpAsException("Unrecognized command line arguments - " + string.Join(' ', remainingArguments));
			}
			if (!s_strategyMap.ContainsKey(m_strategyName))
			{
				throw new ConsoleHelpAsException("Unsupported strategy " + m_strategyName);
			}
			return base.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);
		}

		public override int Run(string[] remainingArguments)
		{
			string templateKey;
			if (m_baseDir == null)
			{
				m_templateFile = Path.GetFullPath(m_templateFile);
				m_baseDir = Path.GetDirectoryName(m_templateFile);
				templateKey = Path.GetFileName(m_templateFile);
			}
			else
			{
				if (!Directory.Exists(m_baseDir))
				{
					throw new RazorLightException($"The razor template base directory {m_baseDir} does not exist.");
				}
				m_baseDir = Path.GetFullPath(m_baseDir);
				if (Path.IsPathRooted(m_templateFile))
				{
					templateKey = Path.GetRelativePath(m_baseDir, m_templateFile);
				}
				else
				{
					templateKey = m_templateFile;
					m_templateFile = Path.GetFullPath(Path.Combine(m_baseDir, m_templateFile));
				}
			}

			if (!File.Exists(m_templateFile))
			{
				throw new RazorLightException($"The razor template file {m_templateFile} does not exist.");
			}

			if (m_cacheDir == null)
			{
				m_cacheDir = Path.GetDirectoryName(m_templateFile);
			}
			else if (!Directory.Exists(m_cacheDir))
			{
				Directory.CreateDirectory(m_cacheDir);
			}

			var provider = new FileSystemCachingProvider(m_baseDir, m_cacheDir, s_strategyMap[m_strategyName]);
			var engine = new RazorLightEngineBuilder()
				.UseFileSystemProject(m_baseDir, "")
				.UseCachingProvider(provider)
				.Build();

			if (m_modelFilePath == null)
			{
				engine.CompileTemplateAsync(templateKey).GetAwaiter().GetResult();
				Program.ConsoleOut.WriteLine(provider.GetAssemblyFilePath(templateKey, m_templateFile));
			}
			else
			{
				var o = JsonConvert.DeserializeObject<JToken>(File.ReadAllText(m_modelFilePath));
				if (m_jsonQuery != null)
				{
					o = o.SelectToken(m_jsonQuery);
				}
				var model = JsonModel.New(o);
				Program.ConsoleOut.WriteLine(engine.CompileRenderAsync(templateKey, model).GetAwaiter().GetResult());
			}
			return 0;
		}
	}
}
