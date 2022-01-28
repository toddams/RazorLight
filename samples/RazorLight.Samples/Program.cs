﻿using Microsoft.EntityFrameworkCore;
using RazorLight;
using System;
using Samples.EntityFrameworkProject.EntityFramework;

namespace Samples.EntityFrameworkProject
{
	class Program
	{
		static void Main()
		{
			var options = new DbContextOptionsBuilder<AppDbContext>()
			   .UseInMemoryDatabase(databaseName: "TestDatabase")
			   .Options;

			// Create and fill database with test data
			var db = new AppDbContext(options);
			FillDatabase(db);


			// Create engine that uses entityFramework to fetch templates from db
			// You can create project that uses your IRepository<T>
			var project = new EntityFrameworkRazorLightProject(db);
			var engine = new RazorLightEngineBuilder().UseProject(project).Build();


			// As our key in database is integer, but engine takes string as a key - pass integer ID as a string
			string templateKey = "2";
			var model = new TestViewModel { Name = "Johny", Age = 22 };

#if (NETCOREAPP3_0 || NETCOREAPP3_1 || NET5_0)
			model.Age = 40;
#endif
			string result = engine.CompileRenderAsync(templateKey, model).Result;

			//Indentation will be a bit fuzzy, as we formatted a string for readability
			Console.WriteLine(result);
			db.Dispose();
		}

		static void FillDatabase(AppDbContext dbContext)
		{
			dbContext.Templates.Add(new TemplateEntity
			{
				Id = 1,
				Content = @"
                    <html>
                        @RenderBody()
                    </html>"
			});

			dbContext.Templates.Add(new TemplateEntity
			{
				Id = 2,
				Content = @"
                    @model Samples.EntityFrameworkProject.TestViewModel
                    @{
                        Layout = 1.ToString(); //This is an ID of your layout in database
                     }
                    <body> Hello, my name is @Model.Name and I am @Model.Age </body>"
			});
		}
	}
}
