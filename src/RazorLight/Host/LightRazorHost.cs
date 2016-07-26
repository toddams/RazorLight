// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
	public class LightRazorHost : RazorEngineHost
    {
        private const string BaseType = "RazorLight.LightRazorPage";
        private const string HtmlHelperPropertyName = "Html";

	    private string _defaultModel = "dynamic";
	    private string _defaultClassName = "__RazorLightTemplate";
	    private string _defaultNamespace = "RazorLight.GeneratedTemplates";

	    private readonly ConfigurationOptions _config;

		private static readonly string[] _defaultNamespaces = new[]
        {
            "System",
            "System.Linq",
            "System.Collections.Generic"
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

		/// <summary>
		/// Initialies LightRazorHost with a specified fileprovider
		/// </summary>
		/// <param name="viewsFileProvider"></param>
	    public LightRazorHost(ConfigurationOptions options) : base(new CSharpRazorCodeLanguage())
		{
			_config = options;

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

        public virtual IReadOnlyList<Chunk> DefaultInheritedChunks
        {
            get { return _defaultInheritedChunks; }
        }

		public virtual string ViewName { get;  set; }

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
						new DefaultChunkTreeCache(_config.ViewsFileProvider));
                }

                return _chunkInheritanceUtility;
            }
            set
            {
                _chunkInheritanceUtility = value;
            }
        }

        /// <inheritdoc />
        public GeneratorResults GenerateCode(string rootRelativePath, Stream inputStream)
        {
            var className = ParserHelpers.SanitizeClassName(rootRelativePath);
            var engine = new RazorTemplateEngine(this);
            return engine.GenerateCode(inputStream, className, DefaultNamespace, rootRelativePath);
        }

        /// <inheritdoc />
        public override ParserBase DecorateCodeParser(ParserBase incomingCodeParser)
        {
            if (incomingCodeParser == null)
            {
                throw new ArgumentNullException(nameof(incomingCodeParser));
            }

            return new LightRazorCodeParser();
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


            IReadOnlyList<ChunkTree> inheritedChunkTrees = GetInheritedChunkTrees(context.SourceFile);

            ChunkInheritanceUtility.MergeInheritedChunkTrees(
                context.ChunkTreeBuilder.Root,
                inheritedChunkTrees,
                DefaultModel);

			return new LightCSharpCodeGenerator(
				context,
				DefaultModel);
        }

        private IReadOnlyList<ChunkTree> GetInheritedChunkTrees(string sourceFileName)
        {
            var inheritedChunkTrees = ChunkInheritanceUtility.GetInheritedChunkTreeResults(sourceFileName)
                .Select(result => result.ChunkTree)
                .ToList();

            return inheritedChunkTrees;
        }
    }
}
