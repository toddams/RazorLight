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
        private readonly RazorLightProject project;

        public TemplateFactoryProvider(
            RazorLightProject razorProject,
            RazorSourceGenerator generator,
            RoslynCompilationService compiler
            )
        {
            sourceGenerator = generator;
            templateCompiler = compiler;
            project = razorProject;
        }

        public async Task<TemplateFactoryResult> CreateFactoryAsync(string templateKey)
        {
            RazorCSharpDocument csharpCodument = await sourceGenerator.GenerateCodeAsync(templateKey).ConfigureAwait(false);

            if (csharpCodument.Diagnostics.Count > 0)
            {
                var builder = new StringBuilder();
                builder.AppendLine("Failed to generate Razor template. See \"Diagnostics\" property for more details");

                foreach (RazorDiagnostic d in csharpCodument.Diagnostics)
                {
                    builder.AppendLine($"- {d.GetMessage()}");
                }

                throw new TemplateGenerationException(builder.ToString(), csharpCodument.Diagnostics);
            }

            Assembly generatedAssembly = templateCompiler.CompileAndEmit(csharpCodument.GeneratedCode);
            var templateDescriptor = new CompiledTemplateDescriptor()
            {
                TemplateKey = templateKey,
                TemplateAttribute = generatedAssembly.GetCustomAttribute<RazorLightTemplateAttribute>(),
            };

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
