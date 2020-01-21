using System;

namespace RazorLight.Compilation
{
	public interface ITemplateFactoryProvider
	{
		Func<ITemplatePage> CreateFactory(CompiledTemplateDescriptor templateDescriptor);
	}
}