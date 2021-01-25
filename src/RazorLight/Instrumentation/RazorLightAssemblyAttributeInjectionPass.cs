using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using System.Diagnostics;

namespace RazorLight.Instrumentation
{
	public class RazorLightAssemblyAttributeInjectionPass : IntermediateNodePassBase, IRazorOptimizationPass
	{
		private const string RazorLightTemplateAttribute = "global::RazorLight.Razor.RazorLightTemplateAttribute";

		protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
		{
			var @namespace = documentNode.FindPrimaryNamespace();
			if (@namespace == null || string.IsNullOrEmpty(@namespace.Content))
			{
				// No namespace node or it's incomplete. Skip.
				return;
			}

			var @class = documentNode.FindPrimaryClass();
			if (@class == null || string.IsNullOrEmpty(@class.ClassName))
			{
				// No class node or it's incomplete. Skip.
				return;
			}

			string generatedTypeName = $"{@namespace.Content}.{@class.ClassName}";
			string templateKey = codeDocument.Source.FilePath;
			string escapedTemplateKey = EscapeAsVerbatimLiteral(templateKey);

			string attribute;
			if (documentNode.DocumentKind == RazorLightTemplateDocumentClassifierPass.RazorLightTemplateDocumentKind)
			{
				attribute = $"[assembly:{RazorLightTemplateAttribute}({escapedTemplateKey}, typeof({generatedTypeName}))]";
			}
			else
			{
				return;
			}

			int index = documentNode.Children.IndexOf(@namespace);
			Debug.Assert(index >= 0);

			var pageAttribute = new CSharpCodeIntermediateNode();
			pageAttribute.Children.Add(new IntermediateToken
			{
				Kind = TokenKind.CSharp,
				Content = attribute,
			});

			documentNode.Children.Insert(index, pageAttribute);
		}

		private static string EscapeAsVerbatimLiteral(string value)
		{
			if (value == null)
			{
				return "null";
			}

			value = value.Replace("\"", "\"\"");
			return $"@\"{value}\"";
		}
	}
}
