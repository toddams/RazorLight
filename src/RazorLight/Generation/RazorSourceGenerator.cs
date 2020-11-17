using Microsoft.AspNetCore.Razor.Language;
using RazorLight.Razor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RazorLight.Generation
{
	public class RazorSourceGenerator
	{
		public RazorSourceGenerator(RazorEngine projectEngine, RazorLightProject project = null, ISet<string> namespaces = null)
		{
			if (projectEngine == null)
			{
				throw new ArgumentNullException(nameof(projectEngine));
			}

			Namespaces = namespaces ?? new HashSet<string>();

			ProjectEngine = projectEngine;
			Project = project;
			DefaultImports = GetDefaultImports();
		}

		public RazorEngine ProjectEngine { get; set; }

		public RazorLightProject Project { get; set; }

		public ISet<string> Namespaces { get; set; }

		public RazorSourceDocument DefaultImports { get; set; }

		/// <summary>
		/// Parses the template specified by the project item <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The template path.</param>
		/// <returns>The <see cref="IGeneratedRazorTemplate"/>.</returns>
		public async Task<IGeneratedRazorTemplate> GenerateCodeAsync(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException();
			}

			if (Project == null)
			{
				string _message = $"Can not resolve a content for the template \"{key}\" as there is no project set. " +
					"You can only render a template by passing it's content directly via string using corresponding function overload";

				throw new InvalidOperationException(_message);
			}

			RazorLightProjectItem projectItem = await Project.GetItemAsync(key).ConfigureAwait(false);
			return await GenerateCodeAsync(projectItem);
		}

		/// <summary>
		/// Parses the template specified by <paramref name="projectItem"/>.
		/// </summary>
		/// <param name="projectItem">The <see cref="RazorLightProjectItem"/>.</param>
		/// <returns>The <see cref="IGeneratedRazorTemplate"/>.</returns>
		public async Task<IGeneratedRazorTemplate> GenerateCodeAsync(RazorLightProjectItem projectItem)
		{
			if (projectItem == null)
			{
				throw new ArgumentNullException(nameof(projectItem));
			}

			if (!projectItem.Exists)
			{
				throw new InvalidOperationException($"{nameof(RazorLightProjectItem)} of type " +
					$"{projectItem.GetType().FullName} with key {projectItem.Key} does not exist.");
			}

			RazorCodeDocument codeDocument = await CreateCodeDocumentAsync(projectItem);
			ProjectEngine.Process(codeDocument);

			RazorCSharpDocument document = codeDocument.GetCSharpDocument();
			if (document.Diagnostics.Count > 0)
			{
				var builder = new StringBuilder();
				builder.AppendLine("Failed to generate Razor template. See \"Diagnostics\" property for more details");

				foreach (RazorDiagnostic d in document.Diagnostics)
				{
					builder.AppendLine($"- {d.GetMessage()}");
				}

				throw new TemplateGenerationException(builder.ToString(), document.Diagnostics);
			}

			return new GeneratedRazorTemplate(projectItem, document);
		}

		/// <summary>
		/// Generates a <see cref="RazorCodeDocument"/> for the specified <paramref name="projectItem"/>.
		/// </summary>
		/// <param name="projectItem">The <see cref="RazorLightProjectItem"/>.</param>
		/// <returns>The created <see cref="RazorCodeDocument"/>.</returns>
		public virtual async Task<RazorCodeDocument> CreateCodeDocumentAsync(RazorLightProjectItem projectItem)
		{
			if (projectItem == null)
			{
				throw new ArgumentNullException(nameof(projectItem));
			}

			if (!projectItem.Exists)
			{
				throw new InvalidOperationException($"{nameof(RazorLightProjectItem)} of type " +
					$"{projectItem.GetType().FullName} with key {projectItem.Key} does not exist.");
			}

			using (var stream = projectItem.Read())
			{
				RazorSourceDocument source = RazorSourceDocument.ReadFrom(stream, projectItem.Key);
				IEnumerable<RazorSourceDocument> imports = await GetImportsAsync(projectItem);

				return RazorCodeDocument.Create(source, imports);
			}
		}

		/// <summary>
		/// Gets <see cref="RazorSourceDocument"/> that are applicable to the specified <paramref name="projectItem"/>.
		/// </summary>
		/// <param name="projectItem">The <see cref="RazorLightProjectItem"/>.</param>
		/// <returns>The sequence of applicable <see cref="RazorSourceDocument"/>.</returns>
		public virtual async Task<IEnumerable<RazorSourceDocument>> GetImportsAsync(RazorLightProjectItem projectItem)
		{
			if (projectItem == null)
			{
				throw new ArgumentNullException(nameof(projectItem));
			}

			if (projectItem is TextSourceRazorProjectItem)
			{
				return Enumerable.Empty<RazorSourceDocument>();
			}

			var result = new List<RazorSourceDocument>();

			IEnumerable<RazorLightProjectItem> importProjectItems = await Project.GetImportsAsync(projectItem.Key);
			foreach (var importItem in importProjectItems)
			{
				if (importItem.Exists)
				{
					using (var stream = importItem.Read())
					{
						result.Insert(0, RazorSourceDocument.ReadFrom(stream, null));
					}
				}
			}

			if (Namespaces != null)
			{
				RazorSourceDocument namespacesImports = GetNamespacesImports();
				if (namespacesImports != null)
				{
					result.Insert(0, namespacesImports);
				}
			}

			if (DefaultImports != null)
			{
				result.Insert(0, DefaultImports);
			}

			return result;
		}

		internal protected RazorSourceDocument GetDefaultImports()
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream, Encoding.UTF8))
			{
				foreach (string line in GetDefaultImportLines())
				{
					writer.WriteLine(line);
				}

				writer.Flush();

				stream.Position = 0;
				return RazorSourceDocument.ReadFrom(stream, fileName: null, encoding: Encoding.UTF8);
			}
		}

		internal protected RazorSourceDocument GetNamespacesImports()
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream, Encoding.UTF8))
			{
				foreach (string @namespace in Namespaces)
				{
					writer.WriteLine($"@using {@namespace}");
				}

				writer.Flush();

				stream.Position = 0;
				return RazorSourceDocument.ReadFrom(stream, fileName: null, encoding: Encoding.UTF8);
			}
		}

		public virtual IEnumerable<string> GetDefaultImportLines()
		{
			yield return "@using System";
			yield return "@using System.Collections.Generic";
			yield return "@using System.Linq";
			yield return "@using System.Threading.Tasks";

			//"@inject global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TModel> Html");
			//"@inject global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json");
			//"@inject global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component");
			//"@inject global::Microsoft.AspNetCore.Mvc.IUrlHelper Url");
			//"@inject global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider");
			//"@addTagHelper Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper, Microsoft.AspNetCore.Mvc.Razor");
			//"@addTagHelper Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper, Microsoft.AspNetCore.Mvc.Razor");
			//"@addTagHelper Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper, Microsoft.AspNetCore.Mvc.Razor");
		}
	}
}
