using Microsoft.AspNetCore.Razor.Language;
using RazorLight.Compilation;
using RazorLight.Razor;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorLight
{
    public class TemplateFactoryProvider : ITemplateFactoryProvider
    {
        private readonly RazorSourceGenerator sourceGenerator;
        private readonly RoslynCompilationService templateCompiler;

        public TemplateFactoryProvider(
            RazorSourceGenerator generator,
            RoslynCompilationService compiler
            )
        {
            sourceGenerator = generator;
            templateCompiler = compiler;
        }

        public async Task<TemplateFactoryResult> CreateFactoryAsync(string templateKey)
        {
            if(templateKey == null)
            {
                throw new ArgumentNullException(nameof(templateKey));
            }

            GeneratedRazorTemplate razorTemplate = await sourceGenerator.GenerateCodeAsync(templateKey).ConfigureAwait(false);

            CompiledTemplateDescriptor templateDescriptor = templateCompiler.CompileAndEmit(razorTemplate);
            if(templateDescriptor.TemplateAttribute != null)
            {
                Type compiledType = templateDescriptor.TemplateAttribute.TemplateType;

                var newExpression = Expression.New(compiledType);
                var keyProperty = compiledType.GetTypeInfo().GetProperty(nameof(ITemplatePage.Key));
                var propertyBindExpression = Expression.Bind(keyProperty, Expression.Constant(templateKey));
                var objectInitializeExpression = Expression.MemberInit(newExpression, propertyBindExpression);

                var pageFactory = Expression
                        .Lambda<Func<ITemplatePage>>(objectInitializeExpression)
                        .Compile();
                return new TemplateFactoryResult(templateDescriptor, pageFactory);
            }
            else
            {
                return new TemplateFactoryResult(templateDescriptor, null);
            }
        }
    }
}
