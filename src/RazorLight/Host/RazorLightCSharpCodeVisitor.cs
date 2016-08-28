using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.CodeGenerators.Visitors;
using System;

namespace RazorLight.Host
{
	public abstract class RazorLightCSharpCodeVisitor : CodeVisitor<CSharpCodeWriter>
	{
		public RazorLightCSharpCodeVisitor(
			CSharpCodeWriter writer,
			CodeGeneratorContext context)
			: base(writer, context)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}
		}

		public override void Accept(Chunk chunk)
		{
			if (chunk is InjectChunk)
			{
				Visit((InjectChunk)chunk);
			}
			else
			{
				base.Accept(chunk);
			}
		}

		protected abstract void Visit(InjectChunk chunk);
	}
}
