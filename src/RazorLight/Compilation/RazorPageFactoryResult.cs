using System;

namespace RazorLight.Compilation
{
	public struct TemplateFactoryResult
	{
		/// <summary>
		/// Initializes a new instance of <see cref="TemplateFactoryResult"/> with the
		/// specified <see cref="ITemplatePage"/> factory.
		/// </summary>
		/// <param name="templatePageFactory">The <see cref="ITemplatePage"/> factory.</param>
		/// <param name="viewDescriptor">The <see cref="CompiledTemplateDescriptor"/>.</param>
		public TemplateFactoryResult(
			CompiledTemplateDescriptor viewDescriptor,
			Func<ITemplatePage> templatePageFactory)
		{
			TemplateDescriptor = viewDescriptor ?? throw new ArgumentNullException(nameof(viewDescriptor));
			TemplatePageFactory = templatePageFactory;
		}

		public CompiledTemplateDescriptor TemplateDescriptor { get; }

		public Func<ITemplatePage> TemplatePageFactory { get; }
	}
}
