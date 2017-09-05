using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using RazorLight.Caching;
using RazorLight.Compilation;
using RazorLight.Instrumentation;
using RazorLight.Internal;
using RazorLight.Razor;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace RazorLight
{
    public class RazorLightEngine
    {
        private RazorEngine razorEngine;
        private RoslynCompiler roslynCompiler;

        private ICachingProvider cache;

        public RazorLightEngine()
        {
            roslynCompiler = new RoslynCompiler();
            SourceGenerator = new RazorSourceGenerator(razorEngine, new FileSystemRazorProject("C:/"));
            razorEngine = RazorEngine.Create(builder =>
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
                //builder.Features.Add(new PagesPropertyInjectionPass());
                //builder.Features.Add(new ViewComponentTagHelperPass());
                builder.Features.Add(new RazorLightTemplateDocumentClassifierPass());

                if (!builder.DesignTime)
                {
                    builder.Features.Add(new RazorLightAssemblyAttributeInjectionPass());
                    builder.Features.Add(new InstrumentationPass());
                }
            });
        }

        public RazorSourceGenerator SourceGenerator { get; set; }

        public void Parse(string key)
        {
            RazorCodeDocument codeDocument = SourceGenerator.CreateCodeDocument(key);
            RazorCSharpDocument csharpCodument = SourceGenerator.GenerateCode(codeDocument);

            if (csharpCodument.Diagnostics.Count > 0)
            {
                throw new Exception("Failed to generate template source");
            }

            Assembly generatedAssembly = compiler.CompileAndEmit(csharpCodument.GeneratedCode);
            var viewAttribute = generatedAssembly.GetCustomAttribute<RazorLightTemplateAttribute>();

            var compiledType = viewAttribute.ViewType;
            var newExpression = Expression.New(compiledType);

            var keyProperty = compiledType.GetTypeInfo().GetProperty(nameof(ITemplatePage.Key));
            var propertyBindExpression = Expression.Bind(keyProperty, Expression.Constant(key));
            var objectInitializeExpression = Expression.MemberInit(newExpression, propertyBindExpression);
            var pageFactory = Expression
                    .Lambda<Func<ITemplatePage>>(objectInitializeExpression)
                    .Compile();

            var template = pageFactory();
            template.SetModel(DateTime.Now);

            var context = new PageContext();
            template.PageContext = context;

            template.ExecuteAsync().GetAwaiter().GetResult();

            var a = context.Writer.ToString();
        }
    }
}
