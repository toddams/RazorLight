// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.CodeGenerators.Visitors;

namespace RazorLight.Host
{
	public class RazorLightCSharpCodeGenerator : CSharpCodeGenerator
	{
		private readonly string _defaultModel;
		private readonly string _injectAttribute;

		public RazorLightCSharpCodeGenerator(
			CodeGeneratorContext context,
			string defaultModel,
			string injectAttribute)
			: base(context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (defaultModel == null)
			{
				throw new ArgumentNullException(nameof(defaultModel));
			}

			if (injectAttribute == null)
			{
				throw new ArgumentNullException(nameof(injectAttribute));
			}

			_defaultModel = defaultModel;
			_injectAttribute = injectAttribute;
		}

		protected override CSharpCodeVisitor CreateCSharpCodeVisitor(
			CSharpCodeWriter writer,
			CodeGeneratorContext context)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			var csharpCodeVisitor = base.CreateCSharpCodeVisitor(writer, context);

			return csharpCodeVisitor;
		}

		protected override CSharpDesignTimeCodeVisitor CreateCSharpDesignTimeCodeVisitor(
			CSharpCodeVisitor csharpCodeVisitor,
			CSharpCodeWriter writer,
			CodeGeneratorContext context)
		{
			if (csharpCodeVisitor == null)
			{
				throw new ArgumentNullException(nameof(csharpCodeVisitor));
			}

			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			return new RazorLightCSharpDesignTimeCodeVisitor(csharpCodeVisitor, writer, context);
		}

		protected override void BuildConstructor(CSharpCodeWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}

			base.BuildConstructor(writer);

			writer.WriteLineHiddenDirective();

			var injectVisitor = new InjectChunkVisitor(writer, Context, _injectAttribute);
			injectVisitor.Accept(Context.ChunkTreeBuilder.Root.Children);

			writer.WriteLine();
			writer.WriteLineHiddenDirective();
		}
	}
}