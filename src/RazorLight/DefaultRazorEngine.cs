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
				var configuration = RazorConfiguration.Default;
				var razorProjectEngine = RazorProjectEngine.Create(configuration, new NullRazorProjectFileSystem(), builder =>
			   {
				   Instrumentation.InjectDirective.Register(builder);
				   Instrumentation.ModelDirective.Register(builder);

				   //In RazorLanguageVersion > 3.0 (at least netcore 3.0) the directives are registered out of the box.
				   if (!RazorLanguageVersion.TryParse("3.0", out var razorLanguageVersion)
					   || configuration.LanguageVersion.CompareTo(razorLanguageVersion) < 0)
				   {
					   NamespaceDirective.Register(builder);
					   FunctionsDirective.Register(builder);
					   InheritsDirective.Register(builder);
				   }
				   SectionDirective.Register(builder);

				   builder.Features.Add(new ModelExpressionPass());
				   builder.Features.Add(new RazorLightTemplateDocumentClassifierPass());
				   builder.Features.Add(new RazorLightAssemblyAttributeInjectionPass());
#if NETSTANDARD2_0
				   builder.Features.Add(new InstrumentationPass());
#endif
				   //builder.Features.Add(new ViewComponentTagHelperPass());

				   builder.AddTargetExtension(new TemplateTargetExtension
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


#if (NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0)
			[System.Obsolete]
#endif
			public override RazorProjectItem GetItem(string path)
			{
				throw new System.NotImplementedException();
			}

#if (NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0)
			public override RazorProjectItem GetItem(string path, string fileKind)
			{
				throw new System.NotImplementedException();
			}
#endif
		}
	}
}
