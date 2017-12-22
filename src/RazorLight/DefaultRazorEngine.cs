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
                return RazorEngine.Create(builder =>
                {
                    Instrumentation.InjectDirective.Register(builder);
                    Instrumentation.ModelDirective.Register(builder);
                    NamespaceDirective.Register(builder);
                    FunctionsDirective.Register(builder);
                    InheritsDirective.Register(builder);
                    SectionDirective.Register(builder);

                    //builder.AddTargetExtension(new TemplateTargetExtension()
                    //{
                    //    TemplateTypeName = "global::Microsoft.AspNetCore.Mvc.Razor.HelperResult",
                    //});

                    builder.Features.Add(new ModelExpressionPass());
                    //builder.Features.Add(new ViewComponentTagHelperPass());
                    builder.Features.Add(new RazorLightTemplateDocumentClassifierPass());

                    if (!builder.DesignTime)
                    {
                        builder.Features.Add(new RazorLightAssemblyAttributeInjectionPass());
                        builder.Features.Add(new InstrumentationPass());
                    }
                });
            }
        }
    }
}
