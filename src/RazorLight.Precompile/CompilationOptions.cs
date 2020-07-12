using Microsoft.Extensions.CommandLineUtils;

namespace RazorLight.Precompile
{
	internal class CompilationOptions
	{
		public static readonly string ContentRootTemplate = "--content-root";
		public static readonly string ApplicationNameTemplate = "--application-name";
		public static readonly string OutputPathTemplate = "--output-path";
		public static readonly string Extension = "--extension";

		public CompilationOptions(CommandLineApplication app)
		{
			OutputPathOption = app.Option(
			   OutputPathTemplate,
				"Path to the emit the precompiled assembly to.",
				CommandOptionType.SingleValue);

			ApplicationNameOption = app.Option(
				ApplicationNameTemplate,
				"Name of the application to produce precompiled assembly for.",
				CommandOptionType.SingleValue);

			ProjectArgument = app.Argument(
				"project",
				"The path to the project file.");

			ContentRootOption = app.Option(
				ContentRootTemplate,
				"The application's content root.",
				CommandOptionType.SingleValue);

			TemplatesExtension = app.Option(
				Extension,
				"Templates extension",
				CommandOptionType.SingleValue);
		}

		public CommandArgument ProjectArgument { get; }

		public CommandOption ContentRootOption { get; }

		public CommandOption OutputPathOption { get; }

		public CommandOption ApplicationNameOption { get; }

		public CommandOption TemplatesExtension { get; }

		public string OutputPath => OutputPathOption.Value();

		public string ApplicationName => ApplicationNameOption.Value();
	}
}
