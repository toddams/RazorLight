using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using RazorLight.Compilation;
using RazorLight.Generation;
using RazorLight.Razor;
using Xunit;

namespace RazorLight.Tests.Compilation
{
	public class RazorTemplateCompilerTest
    {
		[Fact]
		public void Ensure_Throws_OnNull_Constructor_Dependencies()
		{
			var options = new RazorLightOptions();
			var metadataManager = new DefaultMetadataReferenceManager();
			var assembly = Assembly.GetCallingAssembly();
			var project = new EmbeddedRazorProject(assembly);
			var compilerService = new RoslynCompilationService(metadataManager, assembly);
			var generator = new RazorSourceGenerator(DefaultRazorEngine.Instance, project);

			Action p1 = new Action(() => { new RazorTemplateCompiler(null, compilerService, project, options); });
			Action p2 = new Action(() => { new RazorTemplateCompiler(generator, null, project, options); });
			Action p3 = new Action(() => { new RazorTemplateCompiler(generator, compilerService, null, options); });
			Action p4 = new Action(() => { new RazorTemplateCompiler(generator, compilerService, project, null); });

			Assert.Throws<ArgumentNullException>(p1);
			Assert.Throws<ArgumentNullException>(p2);
			Assert.Throws<ArgumentNullException>(p3);
			Assert.Throws<ArgumentNullException>(p4);
		}

		[Fact]
		public void TemplateKey_NotNormalized_OnStringRendering()
		{
			string templateKey = "key";

			var options = new RazorLightOptions();
			options.DynamicTemplates.Add(templateKey, "Template content");
			var compiler = TestRazorTemplateCompiler.Create(options);

			string normalizedKey = compiler.GetNormalizedKey(templateKey);

			Assert.NotNull(normalizedKey);
			Assert.Equal(templateKey, normalizedKey);
		}

		[Fact]
		public void TemplateKey_Normalized_On_FilesystemProject()
		{
			string templateKey = "key";
			var project = new FileSystemRazorProject("/");
			var compiler = TestRazorTemplateCompiler.Create(project: project);

			string normalizedKey = compiler.GetNormalizedKey(templateKey);

			Assert.NotNull(normalizedKey);
			Assert.Equal($"/{templateKey}", normalizedKey);
		}

		[Fact]
		public void TemplateKey_NotNormalized_On_NonFileSystemProject()
		{
			string templateKey = "key";
			var project = new EmbeddedRazorProject(typeof(Root).Assembly);
			var compiler = TestRazorTemplateCompiler.Create(project: project);

			string normalizedKey = compiler.GetNormalizedKey(templateKey);

			Assert.NotNull(normalizedKey);
			Assert.Equal(templateKey, normalizedKey);
		}

		[Fact]
		public async Task Compiler_Takes_Result_From_Cache_OnCompileAsync()
		{
			string templateKey = "key";
			var descriptor = new CompiledTemplateDescriptor();
			var descriptorTask = Task.FromResult(descriptor);

			var compiler = TestRazorTemplateCompiler.Create();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			compiler.Cache.Set(templateKey, descriptorTask);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			CompiledTemplateDescriptor result = await compiler.CompileAsync(templateKey);

			Assert.NotNull(result);
			Assert.Same(descriptor, result);
		}

		[Fact]
		public async Task Compiler_Searches_WithNormalizedKey_IfNotFound()
		{
			string templateKey = "key";
			var descriptor = new CompiledTemplateDescriptor();
			var descriptorTask = Task.FromResult(descriptor);

			var project = new FileSystemRazorProject("/");
			var compiler = TestRazorTemplateCompiler.Create(project: project);

			string normalizedKey = compiler.GetNormalizedKey(templateKey);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			compiler.Cache.Set(normalizedKey, descriptorTask);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			CompiledTemplateDescriptor result = await compiler.CompileAsync(templateKey);

			Assert.NotNull(result);
			Assert.Same(descriptor, result);
		}

		[Fact]
		public void Throws_TemplateNotFoundException_If_ProjectItem_NotExist()
		{
			var project = new EmbeddedRazorProject(typeof(Root).Assembly);
			var compiler = TestRazorTemplateCompiler.Create(project:project);

			Func<Task> task = new Func<Task>(() => compiler.CompileAsync("Not.Existing.Key"));

			Assert.ThrowsAsync<TemplateNotFoundException>(task);
		}

		[Fact]
		public async Task Ensure_TemplateIsCompiled_ForExisting_ProjectItem()
		{
			var project = new EmbeddedRazorProject(typeof(Root).Assembly, "RazorLight.Tests.Assets.Embedded");
			var compiler = TestRazorTemplateCompiler.Create(project:project);

			string templateKey = "Empty.cshtml";
			var result = await compiler.CompileAsync(templateKey);

			Assert.NotNull(result);
			Assert.NotNull(result.TemplateAttribute.TemplateType);
			Assert.Equal(result.TemplateKey, templateKey);
			Assert.False(result.IsPrecompiled);
		}



		public class TestRazorTemplateCompiler : RazorTemplateCompiler
		{
			public TestRazorTemplateCompiler(
				RazorSourceGenerator sourceGenerator, 
				RoslynCompilationService roslynCompilationService, 
				RazorLightProject razorLightProject, 
				RazorLightOptions razorLightOptions) : base(sourceGenerator, roslynCompilationService, razorLightProject, razorLightOptions)
			{
			}

			public static TestRazorTemplateCompiler Create(RazorLightOptions options = null, RazorLightProject project = null)
			{
				var razorOptions = options ?? new RazorLightOptions();
				var metatadaManager = new DefaultMetadataReferenceManager();
				var assembly = Assembly.GetCallingAssembly();
				var razorProject = project ?? new EmbeddedRazorProject(assembly);
				var compilerService = new RoslynCompilationService(metatadaManager, assembly);
				var generator = new RazorSourceGenerator(DefaultRazorEngine.Instance, razorProject);

				return new TestRazorTemplateCompiler(generator, compilerService, razorProject, razorOptions);
			}
		}
	}
}
