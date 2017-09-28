using RazorLight.Compilation;
using RazorLight.Razor;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace RazorLight
{
    public class TemplateFactoryProvider : ITemplateFactoryProvider
    {
        private RazorLightOptions options;
        private readonly RazorSourceGenerator sourceGenerator;
        private readonly RoslynCompilationService templateCompiler;

        public TemplateFactoryProvider(
            RazorSourceGenerator generator,
            RoslynCompilationService compiler,
            RazorLightOptions razorOptions)
        {
            sourceGenerator = generator ?? throw new ArgumentNullException(nameof(generator));
            templateCompiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            options = razorOptions ?? throw new ArgumentNullException(nameof(razorOptions));
        }

        public async Task<TemplateFactoryResult> CreateFactoryAsync(string templateKey)
        {
            if (templateKey == null)
            {
                throw new ArgumentNullException(nameof(templateKey));
            }

            GeneratedRazorTemplate razorTemplate = null;

            if (options.DynamicTemplates.TryGetValue(templateKey, out string templateContent))
            {
                var projectItem = new TextSourceRazorProjectItem(templateKey, templateContent);
                razorTemplate = await sourceGenerator.GenerateCodeAsync(projectItem).ConfigureAwait(false);
            }
            else
            {
                razorTemplate = await sourceGenerator.GenerateCodeAsync(templateKey).ConfigureAwait(false);
            }

            return CreateFactory(razorTemplate);
        }

        public async Task<TemplateFactoryResult> CreateFactoryAsync(RazorLightProjectItem projectItem)
        {
            if(projectItem == null)
            {
                throw new ArgumentNullException(nameof(projectItem));
            }

            GeneratedRazorTemplate razorTemplate = await sourceGenerator.GenerateCodeAsync(projectItem).ConfigureAwait(false);

            return CreateFactory(razorTemplate);
        }

        protected TemplateFactoryResult CreateFactory(GeneratedRazorTemplate razorTemplate)
        {
            CompiledTemplateDescriptor templateDescriptor = templateCompiler.CompileAndEmit(razorTemplate);
            templateDescriptor.ExpirationToken = razorTemplate.ProjectItem.ExpirationToken;

            string templateKey = templateDescriptor.TemplateKey;

            if (templateDescriptor.TemplateAttribute != null)
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
                throw new RazorLightException($"Template {templateKey} is corrupted or invalid");
            }
        }
    }
}
