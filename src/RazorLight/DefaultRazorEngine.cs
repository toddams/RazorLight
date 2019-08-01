using System.Collections.Generic;
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
				var razorProjectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, new NullRazorProjectFileSystem() , builder =>
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

					builder.AddTargetExtension(new TemplateTargetExtension()
					{
						TemplateTypeName = "global::RazorLight.Razor.RazorLightHelperResult",
					});

					OverrideRuntimeNodeWriterTemplateTypeNamePhase.Register(builder);
				});

				return razorProjectEngine.Engine;
			}
		}

		private class NullRazorProjectFileSystem : RazorProjectFileSystem
		{
			public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath)
			{
				throw new System.NotImplementedException();
			}

			public override RazorProjectItem GetItem(string path)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
