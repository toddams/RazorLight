using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RazorLight.Tests.Integrational
{
	public class RaceConditionTests
	{
		[Fact]
		public async Task Multiple_Simultaneous_Compilations_RaceCondition_Test()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


			for (int i = 0; i < 1000; i++)
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
