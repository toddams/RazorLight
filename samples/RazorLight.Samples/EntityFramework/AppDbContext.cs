﻿using Microsoft.EntityFrameworkCore;

namespace Samples.EntityFrameworkProject.EntityFramework
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<TemplateEntity> Templates { get; set; }
	}
}
