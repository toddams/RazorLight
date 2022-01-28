﻿using System;
using System.IO;
using System.Threading.Tasks;
using RazorLight.Tests.Utils;
using VerifyXunit;
using Xunit;

namespace RazorLight.Tests.Integration
{
	public class TestViewModel
	{
		public string Name { get; set; }

		public int NumberOfItems { get; set; }
	}

	[UsesVerify]
	public class RendererCommonCasesTests
	{

		[Fact()]
		public async Task Should_Render_Section_And_ViewModel()
		{
			var path = DirectoryUtils.RootDirectory;

			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
					.SetOperatingAssembly(typeof(Root).Assembly)
#endif
					.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
					.Build();

			var model = new TestViewModel
			{
				Name = "RazorLight",
				NumberOfItems = 100
			};
			var renderedResult = await engine.CompileRenderAsync("template4.cshtml", model);
			await Verifier.Verify(renderedResult);
		}

		[Fact()]
		public async Task Should_Render_Sections_With_IncludeAsync()
		{
			var path = DirectoryUtils.RootDirectory;

			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel
			{
				Name = "RazorLight",
				NumberOfItems = 200
			};
			var renderedResult = await engine.CompileRenderAsync("template5.cshtml", model);
			await Verifier.Verify(renderedResult);
		}

		[Fact()]
		public Task Should_Fail_When_Required_Section_Is_Missing()
		{
			var path = DirectoryUtils.RootDirectory;

			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel
			{
				Name = "RazorLight",
				NumberOfItems = 300
			};
			return Verifier.ThrowsTask(() => engine.CompileRenderAsync("template6.cshtml", model))
				.ModifySerialization(_ => _.IgnoreMember<Exception>(exception => exception.StackTrace));
		}

		[Fact]
		public async Task Should_Render_IncludeAsync()
		{
			var path = DirectoryUtils.RootDirectory;

			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel
			{
				Name = "RazorLight",
				NumberOfItems = 400
			};
			var renderedResult = await engine.CompileRenderAsync("template7.cshtml", model);
			await Verifier.Verify(renderedResult);
		}

		[Fact]
		public async Task Should_Render_Nested_IncludeAsync()
		{
			var path = DirectoryUtils.RootDirectory;

			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel
			{
				Name = "RazorLight",
				NumberOfItems = 400
			};
			var renderedResult = await engine.CompileRenderAsync("template9.cshtml", model);
			await Verifier.Verify(renderedResult);
		}

		[Fact()]
		public async Task Should_Render_RequiredSections_That_Have_Nested_IncludeAsync()
		{
			var path = DirectoryUtils.RootDirectory;

			var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
				.SetOperatingAssembly(typeof(Root).Assembly)
#endif
				.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
				.Build();

			var model = new TestViewModel
			{
				Name = "RazorLight",
				NumberOfItems = 400
			};
			var renderedResult = await engine.CompileRenderAsync("template8.cshtml", model);
			await Verifier.Verify(renderedResult);
		}
	}
}