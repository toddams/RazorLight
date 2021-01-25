using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using RazorLight.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RazorLight.Instrumentation
{
	public class InjectDirective
	{
		public static readonly DirectiveDescriptor Directive = DirectiveDescriptor.CreateDirective(
			"inject",
			DirectiveKind.SingleLine,
			builder =>
			{
				builder.AddTypeToken().AddMemberToken();
				builder.Usage = DirectiveUsage.FileScopedMultipleOccurring;
				builder.Description = ""; //TODO: add description
			});

		public static RazorProjectEngineBuilder Register(RazorProjectEngineBuilder builder)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			builder.AddDirective(Directive);
			builder.Features.Add(new Pass());
			builder.AddTargetExtension(new InjectTargetExtension());
			return builder;
		}
		internal class Pass : IntermediateNodePassBase, IRazorDirectiveClassifierPass
		{
			// Runs after the @model and @namespace directives
			public override int Order => 10;

			protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
			{
				var visitor = new Visitor();
				visitor.Visit(documentNode);
				var modelType = Instrumentation.ModelDirective.GetModelType(documentNode);

				var properties = new HashSet<string>(StringComparer.Ordinal);

				for (var i = visitor.Directives.Count - 1; i >= 0; i--)
				{
					var directive = visitor.Directives[i];
					var tokens = directive.Tokens.ToArray();
					if (tokens.Length < 2)
					{
						continue;
					}

					var typeName = tokens[0].Content;
					var memberName = tokens[1].Content;

					if (!properties.Add(memberName))
					{
						continue;
					}

					typeName = typeName.Replace("<TModel>", "<" + modelType + ">");

					var injectNode = new InjectIntermediateNode
					{
						TypeName = typeName,
						MemberName = memberName,
					};

					visitor.Class.Children.Add(injectNode);
				}
			}
		}

		internal class InjectTargetExtension : IInjectTargetExtension
		{
			private const string RazorInjectAttribute = "[global::RazorLight.Internal.RazorInjectAttribute]";

			public void WriteInjectProperty(CodeRenderingContext context, InjectIntermediateNode node)
			{
				if (context == null)
				{
					throw new ArgumentNullException(nameof(context));
				}

				if (node == null)
				{
					throw new ArgumentNullException(nameof(node));
				}

				var property = $"public {node.TypeName} {node.MemberName} {{ get; private set; }}";

				if (node.Source.HasValue)
				{
					using (context.CodeWriter.BuildLinePragma(node.Source.Value))
					{
						context.CodeWriter
							.WriteLine(RazorInjectAttribute)
							.WriteLine(property);
					}
				}
				else
				{
					context.CodeWriter
						.WriteLine(RazorInjectAttribute)
						.WriteLine(property);
				}
			}
		}

		private class Visitor : IntermediateNodeWalker
		{
			public ClassDeclarationIntermediateNode Class { get; private set; }

			public IList<DirectiveIntermediateNode> Directives { get; } = new List<DirectiveIntermediateNode>();

			public override void VisitClassDeclaration(ClassDeclarationIntermediateNode node)
			{
				if (Class == null)
				{
					Class = node;
				}

				base.VisitClassDeclaration(node);
			}

			public override void VisitDirective(DirectiveIntermediateNode node)
			{
				if (node.Directive == Directive)
				{
					Directives.Add(node);
				}
			}
		}
	}
}
