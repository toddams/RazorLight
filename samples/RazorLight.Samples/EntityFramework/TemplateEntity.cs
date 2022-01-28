using System.ComponentModel.DataAnnotations;

namespace Samples.EntityFrameworkProject.EntityFramework
{
	//This is simple POCO that represents your template that is stored in database
	public class TemplateEntity
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Content { get; set; }
	}
}
