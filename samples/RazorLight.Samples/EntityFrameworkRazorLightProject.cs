using RazorLight.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
			// We expect id to be an integer, as in this sample we have ints as keys in database.
			// But you can use GUID, as an example and parse it here
			int templateId = int.Parse(templateKey);

			TemplateEntity template = await dbContext.Templates.FindAsync(templateId);

			var projectItem = new EntityFrameworkRazorProjectItem(templateKey, template?.Content);

			return projectItem;
		}

		public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
		{
			return Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
		}

		public override async Task<IEnumerable<string>> GetKnownKeysAsync()
		{
			var ids = await dbContext.Templates.Select(x => x.Id).ToListAsync();
			return ids.Select(x => x.ToString());
		}
	}
}
