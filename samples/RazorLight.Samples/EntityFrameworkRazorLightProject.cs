using RazorLight.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.EntityFrameworkProject
{
    public class EntityFrameworkRazorLightProject : RazorLightProject
    {
        private readonly AppDbContext dbContext;

        public EntityFrameworkRazorLightProject(AppDbContext context)
        {
            dbContext = context;
        }

        public override async Task<RazorLightProjectItem> GetItemAsync(string templateKey)
        {
            int templateId = int.Parse(templateKey);

            TemplateEntity template = await dbContext.Templates.FindAsync(templateId);

            var projectItem = new EntityFrameworkRazorProjectItem(templateKey, template?.Content);

            return projectItem;
        }

        public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
        {
            return Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
        }
    }
}
