using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using RazorLight.Instrumentation;

namespace RazorLight
{
    internal sealed class DefaultRazorEngine
    {
		public static RazorEngine Instance
		{
			get
			{
				var razorProjectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, null, builder =>
				{
					Instrumentation.InjectDirective.Register(builder);
					Instrumentation.ModelDirective.Register(builder);

					NamespaceDirective.Register(builder);
					FunctionsDirective.Register(builder);
					InheritsDirective.Register(builder);
					SectionDirective.Register(builder);

					builder.Features.Add(new ModelExpressionPass());
					builder.Features.Add(new RazorLightTemplateDocumentClassifierPass());
					builder.Features.Add(new RazorLightAssemblyAttributeInjectionPass());
					builder.Features.Add(new InstrumentationPass());


					//builder.Features.Add(new ViewComponentTagHelperPass());


					//builder.AddTargetExtension(new TemplateTargetExtension()
					//{
					//    TemplateTypeName = "global::Microsoft.AspNetCore.Mvc.Razor.HelperResult",
					//});

				});

				return razorProjectEngine.Engine;
			}
		}
    }
}
