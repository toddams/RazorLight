using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using RazorLight;
using RazorLight.Caching;
using RazorLight.Templating;
using RazorLight.Templating.FileSystem;

namespace Sandbox
{
	public class Program
	{
		public static void Main(string[] args)
		{
			string root = @"D:\MyProjects\RazorLight\sandbox\Sandbox\Views\LayoutSections";
			var views = new PhysicalFileProvider(root);

			var engine = new EngineCore(new FilesystemTemplateManager(root));

			string result =
				engine.GenerateRazorTemplate(new FileTemplateSource(
					views.GetFileInfo("With_Layout.cshtml"), "With_Layout.cshtml"), new ModelTypeInfo(typeof(TestViewModel)));

			System.IO.File.WriteAllText(Path.Combine(root, "With_Layout.txt"), result);
		}
	}
}
