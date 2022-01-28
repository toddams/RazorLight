﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using RazorLight.Generation;
using RazorLight.Razor;
using Xunit;

namespace RazorLight.Tests.Generation
{
	public class RazorSourceGeneratorTest
	{
		private RazorSourceGenerator NewGenerator()
		{
			return new RazorSourceGenerator(
				RazorEngine.Create(),
				new EmbeddedRazorProject(typeof(RazorSourceGeneratorTest)));
		}

		[Fact]
		public void Throws_On_Null_Project()
		{
			Action action = () => new RazorSourceGenerator(null);

			Assert.Throws<ArgumentNullException>(action);
		}

		[Fact]
		public void Namespaces_NotNull_If_Not_Specified()
		{
			var generator = new RazorSourceGenerator(
				DefaultRazorEngine.Instance,
				new EmbeddedRazorProject(typeof(Root)),
				namespaces: null);

			Assert.NotNull(generator.Namespaces);
		}

		[Fact]
		public void Ensure_Engine_And_Project_Not_Null()
		{
			var generator = new RazorSourceGenerator(
				RazorEngine.Create(),
				new EmbeddedRazorProject(typeof(RazorSourceGeneratorTest)));

			Assert.NotNull(generator.ProjectEngine);
			Assert.NotNull(generator.Project);
		}

		[Fact]
		public void DefaultImports_Created_On_Constructor()
		{
			var generator = new RazorSourceGenerator(
			   RazorEngine.Create(),
			   new EmbeddedRazorProject(typeof(RazorSourceGeneratorTest)));

			var defaultImports = generator.GetDefaultImportLines().ToList();

			Assert.NotEmpty(defaultImports);

			Assert.Contains(defaultImports, i => i == "@using System");
			Assert.Contains(defaultImports, i => i == "@using System.Threading.Tasks");
			Assert.Contains(defaultImports, i => i == "@using System.Collections.Generic");
			Assert.Contains(defaultImports, i => i == "@using System.Linq");
		}

		//TODO: add tests for imports, etc
		[Fact]
		public void Ensure_Namespaces_Are_Passed()
		{
			var namespaces = new HashSet<string>
			{
				"System.Diagnostics",
				"System.CodeDom"
			};

			var generator = new RazorSourceGenerator(RazorEngine.Create(), new EmbeddedRazorProject(typeof(Root)), namespaces);

			Assert.NotNull(generator.Namespaces);
			Assert.Equal(generator.Namespaces, namespaces);
		}

		[Fact]
		public void Allow_Null_Project()
		{
			var generator = new RazorSourceGenerator(DefaultRazorEngine.Instance, project: null);

			Assert.NotNull(generator);
		}

		[Fact]
		public async Task Return_Empty_Imports_ForTextSource_ProjectItem()
		{
			//Assign
			var generator = new RazorSourceGenerator(RazorEngine.Create(), new EmbeddedRazorProject(typeof(Root)));

			//Act
			var projectItem = new TextSourceRazorProjectItem("key", "some content");
			IEnumerable<RazorSourceDocument> result = await generator.GetImportsAsync(projectItem);

			//Assert
			Assert.NotNull(result);
			Assert.Empty(result);
		}

		[Fact]
		public async Task GetImports_Returns_EmptyCollection_On_Empty_Project_WhenResolving_Content_ByKey()
		{
			//Assign
			var generator = new RazorSourceGenerator(RazorEngine.Create(), new EmbeddedRazorProject(typeof(Root)));

			//Act
			var projectItem = new TextSourceRazorProjectItem("key", "some content");
			var result = await generator.GetImportsAsync(projectItem);

			Assert.NotNull(result);
			Assert.Empty(result);
		}

		[Fact]
		public async Task GenerateCode_ByKey_Throws_OnEmpty_Project()
		{
			var generator = new RazorSourceGenerator(DefaultRazorEngine.Instance, project: null);

			Func<Task> action = () => generator.GenerateCodeAsync("key");

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
			Assert.Equal("Can not resolve a content for the template \"key\" as there is no project set. You can only render a template by passing it's content directly via string using corresponding function overload", exception.Message);
		}

		[Fact]
		public async Task GenerateCode_ByProjectItem_Throws_On_Null_ProjectItem()
		{
			var generator = new RazorSourceGenerator(DefaultRazorEngine.Instance, project: null);

			Func<Task> action = () => generator.GenerateCodeAsync((RazorLightProjectItem)null);

			var exception = await Assert.ThrowsAsync<ArgumentNullException>(action);
			Assert.Equal("projectItem", exception.ParamName);
		}

		[Fact]
		public async Task GenerateCode_ByProjectItem_Throws_On_ProjectItem_Not_Exists()
		{
			var generator = new RazorSourceGenerator(DefaultRazorEngine.Instance, project: null);

			string templateKey = "Assets.Embedded.IDoNotExist.cshtml";

			var projectItem = new EmbeddedRazorProjectItem(typeof(Root), templateKey);

			Assert.False(projectItem.Exists);

			Func<Task> action = () => generator.GenerateCodeAsync(projectItem);

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
			Assert.Equal($"{ nameof(RazorLightProjectItem)} of type {projectItem.GetType().FullName} with key {projectItem.Key} does not exist.", exception.Message);
		}

		[Fact]
		public async Task CreateCodeDocumentAsync_Throws_On_Null_ProjectItem()
		{
			var generator = new RazorSourceGenerator(DefaultRazorEngine.Instance, project: null);

			Func<Task> action = () => generator.CreateCodeDocumentAsync(null);

			var exception = await Assert.ThrowsAsync<ArgumentNullException>(action);
			Assert.Equal("projectItem", exception.ParamName);
		}

		[Fact]
		public async Task CreateCodeDocumentAsync_Throws_On_ProjectItem_Not_Exists()
		{
			var generator = new RazorSourceGenerator(DefaultRazorEngine.Instance, project: null);

			string templateKey = "Assets.Embedded.IDoNotExist.cshtml";

			var projectItem = new EmbeddedRazorProjectItem(typeof(Root), templateKey);

			Assert.False(projectItem.Exists);

			Func<Task> action = () => generator.CreateCodeDocumentAsync(projectItem);

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
			Assert.Equal($"{ nameof(RazorLightProjectItem)} of type {projectItem.GetType().FullName} with key {projectItem.Key} does not exist.", exception.Message);
		}
	}
}
