using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Parser;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;
using RazorLight.Host.Directives;

namespace RazorLight.Host
{
	public class RazorLightHost : RazorEngineHost
	{
		#region "Fields"

		private const string BaseType = "RazorLight.TemplatePage";
		private const string HtmlHelperPropertyName = "Html";

		private string _defaultModel = "dynamic";
		private string _defaultClassName = "__RazorLightTemplate";
		private string _defaultNamespace = "RazorLight.GeneratedTemplates";


		private static readonly string[] _defaultNamespaces = new[]
		{
			"System",
			"System.Linq",
			"System.Collections.Generic",
			"RazorLight.Internal"
		};

		private static readonly Chunk[] _defaultInheritedChunks = new Chunk[]
		{
			new SetBaseTypeChunk
			{
				TypeName = $"{BaseType}<{ChunkHelper.TModelToken}>",
				Start = SourceLocation.Undefined
			}
		};

		private ChunkInheritanceUtility _chunkInheritanceUtility;

		private readonly IFileProvider _viewsFileProvider;

		#endregion

		/// <summary>
		/// Initialies LightRazorHost with a specified fileprovider
		/// </summary>
		public RazorLightHost(IFileProvider viewsFileProvider) : base(new CSharpRazorCodeLanguage())
		{
			this._viewsFileProvider = viewsFileProvider;

			DefaultClassName = _defaultClassName;
			DefaultNamespace = _defaultNamespace;
			DefaultBaseClass = $"{BaseType}<{ChunkHelper.TModelToken}>";
			EnableInstrumentation = false; //This should not be true, unsell you want your code to work :)
			GeneratedClassContext = new GeneratedClassContext(
				executeMethodName: "ExecuteAsync",
				writeMethodName: "Write",
				writeLiteralMethodName: "WriteLiteral",
				writeToMethodName: "WriteTo",
				writeLiteralToMethodName: "WriteLiteralTo",
				templateTypeName: "Microsoft.AspNetCore.Mvc.Razor.HelperResult",
				defineSectionMethodName: "DefineSection",
				generatedTagHelperContext: new GeneratedTagHelperContext
				{
					ExecutionContextTypeName = typeof(TagHelperExecutionContext).FullName,
					ExecutionContextAddMethodName = nameof(TagHelperExecutionContext.Add),
					ExecutionContextAddTagHelperAttributeMethodName =
						nameof(TagHelperExecutionContext.AddTagHelperAttribute),
					ExecutionContextAddHtmlAttributeMethodName = nameof(TagHelperExecutionContext.AddHtmlAttribute),
					ExecutionContextOutputPropertyName = nameof(TagHelperExecutionContext.Output),

					RunnerTypeName = typeof(TagHelperRunner).FullName,
					RunnerRunAsyncMethodName = nameof(TagHelperRunner.RunAsync),

					ScopeManagerTypeName = typeof(TagHelperScopeManager).FullName,
					ScopeManagerBeginMethodName = nameof(TagHelperScopeManager.Begin),
					ScopeManagerEndMethodName = nameof(TagHelperScopeManager.End),

					TagHelperContentTypeName = typeof(TagHelperContent).FullName,

					// Can't use nameof because RazorPage is not accessible here.
					CreateTagHelperMethodName = "CreateTagHelper",
					FormatInvalidIndexerAssignmentMethodName = "InvalidTagHelperIndexerAssignment",
					StartTagHelperWritingScopeMethodName = "StartTagHelperWritingScope",
					EndTagHelperWritingScopeMethodName = "EndTagHelperWritingScope",
					BeginWriteTagHelperAttributeMethodName = "BeginWriteTagHelperAttribute",
					EndWriteTagHelperAttributeMethodName = "EndWriteTagHelperAttribute",

					// Can't use nameof because IHtmlHelper is (also) not accessible here.
					MarkAsHtmlEncodedMethodName = HtmlHelperPropertyName + ".Raw",
					BeginAddHtmlAttributeValuesMethodName = "BeginAddHtmlAttributeValues",
					EndAddHtmlAttributeValuesMethodName = "EndAddHtmlAttributeValues",
					AddHtmlAttributeValueMethodName = "AddHtmlAttributeValue",
					HtmlEncoderPropertyName = "HtmlEncoder",
					TagHelperContentGetContentMethodName = nameof(TagHelperContent.GetContent),
					TagHelperOutputIsContentModifiedPropertyName = nameof(TagHelperOutput.IsContentModified),
					TagHelperOutputContentPropertyName = nameof(TagHelperOutput.Content),
					ExecutionContextSetOutputContentAsyncMethodName = nameof(TagHelperExecutionContext.SetOutputContentAsync),
					TagHelperAttributeValuePropertyName = nameof(TagHelperAttribute.Value),
				})
			{
				BeginContextMethodName = "BeginContext",
				EndContextMethodName = "EndContext"
			};

			foreach (var ns in _defaultNamespaces)
			{
				NamespaceImports.Add(ns);
			}
		}

		/// <summary>
		/// Gets the model type used by default when no model is specified.
		/// </summary>
		/// <remarks>This value is used as the generic type argument for the base type </remarks>
		public virtual string DefaultModel
		{
			get
			{
				return _defaultModel;
			}
			set
			{
				_defaultModel = value;
			}
		}

		/// <summary>
		/// Gets or sets the name attribute that is used to decorate properties that are injected and need to be
		/// activated.
		/// </summary>
		public virtual string InjectAttribute
		{
			get { return "RazorLight.Host.Internal.RazorInjectAttribute"; }
		}

		public virtual IReadOnlyList<Chunk> DefaultInheritedChunks
		{
			get { return _defaultInheritedChunks; }
		}

		internal ChunkInheritanceUtility ChunkInheritanceUtility
		{
			get
			{
				if (_chunkInheritanceUtility == null)
				{
					// This needs to be lazily evaluated to support DefaultInheritedChunks being virtual.
					_chunkInheritanceUtility = new ChunkInheritanceUtility(
						this,
						DefaultInheritedChunks,
						new DefaultChunkTreeCache(_viewsFileProvider));
				}

				return _chunkInheritanceUtility;
			}
			set
			{
				_chunkInheritanceUtility = value;
			}
		}

		public override ParserBase DecorateCodeParser(ParserBase incomingCodeParser)
		{
			if (incomingCodeParser == null)
			{
				throw new ArgumentNullException(nameof(incomingCodeParser));
			}

			return new RazorLightCodeParser();
		}

		/// <inheritdoc />
		public override CodeGenerator DecorateCodeGenerator(
			CodeGenerator incomingGenerator,
			CodeGeneratorContext context)
		{
			if (incomingGenerator == null)
			{
				throw new ArgumentNullException(nameof(incomingGenerator));
			}

			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			IReadOnlyList<ChunkTree> inheritedChunkTrees = new List<ChunkTree>();

			//TODO: add support for viewimports
			//Evaluate inherited chunks only for physycal files.
			//If context.SourceFile is null - we are parsing a string
			//and ViewImports will not be applied
			if (!string.IsNullOrEmpty(context.SourceFile) && this._viewsFileProvider != null)
			{
				inheritedChunkTrees = ChunkInheritanceUtility
				.GetInheritedChunkTreeResults(context.SourceFile)
				.Select(result => result.ChunkTree)
				.ToList();
			}

			ChunkInheritanceUtility.MergeInheritedChunkTrees(
				context.ChunkTreeBuilder.Root,
				inheritedChunkTrees,
				DefaultModel);

			return new RazorLightCSharpCodeGenerator(
				context,
				DefaultModel,
				InjectAttribute);
		}
	}
}
