using System.IO;
using System.Threading.Tasks;
using RazorLight.Tests.Utils;
using Xunit;

namespace RazorLight.Tests.Integration
{
	public class RaceConditionTests
	{
		[Fact]
		public async Task Multiple_Simultaneous_Compilations_RaceCondition_Test()
		{
			var path = DirectoryUtils.RootDirectory;


			for (int i = 0; i < 100; i++)
			{
				var engine = new RazorLightEngineBuilder()
#if NETFRAMEWORK
					.SetOperatingAssembly(typeof(Root).Assembly)
#endif
					.UseFileSystemProject(Path.Combine(path, "Assets", "Files"))
					.Build();

				var model = new { };
				var t1 = Task.Run(async () => await engine.CompileRenderAsync("template1.cshtml", model));
				var t2 = Task.Run(async () => await engine.CompileRenderAsync("template2.cshtml", model));

				var result = await Task.WhenAll(t1, t2);
			}
		}
	}
}
