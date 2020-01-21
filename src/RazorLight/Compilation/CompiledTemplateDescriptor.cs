using System;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.Primitives;
using RazorLight.Razor;

namespace RazorLight.Compilation
{
	public class CompiledTemplateDescriptor
	{
		public string TemplateKey { get; set; }

		public RazorLightTemplateAttribute TemplateAttribute { get; set; }

		public IChangeToken ExpirationToken { get; set; }

		public bool IsPrecompiled { get; set; }

		/// <summary>
		/// Gets the <see cref="RazorCompiledItem"/> descriptor for this view.
		/// </summary>
		public RazorCompiledItem Item { get; set; }

		/// <summary>
		/// Gets the type of the compiled item.
		/// </summary>
		public Type Type => Item?.Type ?? TemplateAttribute?.TemplateType;
	}
}
