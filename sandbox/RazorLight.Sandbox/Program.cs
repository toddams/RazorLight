using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ZstdNet;

namespace RazorLight.Sandbox
{
	class Program
	{
		static async Task<byte[]> UnwrapStream(DecompressionStream decompressionStream)
		{
			var result = new byte[64];
			await decompressionStream.ReadAsync(result, 0, 64);
			return result;
		}
		public static async Task Main()
		{
			var engine = new RazorLightEngineBuilder()
				.UseMemoryCachingProvider()
				.UseEmbeddedResourcesProject(typeof(Program).Assembly, rootNamespace: "RazorLight.Sandbox.Views")
				.Build();

			//string result = await engine.CompileRenderAsync<object>("Home", null, null);
			//Console.WriteLine(result);

			await using var tempStream = new MemoryStream();
			await using var compressionStream = new CompressionStream(tempStream);
			await GetStream(64).CopyToAsync(compressionStream);
			await using var decompressionStream = new DecompressionStream(tempStream);
			await UnwrapStream(decompressionStream);
			//System.GC.SuppressFinalize(decompressionStream);
			var model = new {UnwrappedValue = new Lazy<string>(() =>
			{
				var x = UnwrapStream(decompressionStream).Result;
				return Encoding.Default.GetString(x);
			})};

			string dynamicTemplateResult =
				await engine.CompileRenderStringAsync("templateKey", @"@using RazorLight
@Model.UnwrappedValue.Value", model, null);
			Console.WriteLine(dynamicTemplateResult);

			Console.WriteLine("Finished");
		}

		public static MemoryStream GetStream(int length) => new MemoryStream(GetBuffer(length));
		private static readonly Random Random = new Random(1234);
		public static byte[] GetBuffer(int length)
		{
			var buffer = new byte[length];
			Random.NextBytes(buffer);

			return buffer;
		}

		private static readonly object locker = new object();

		private static int _j;
		public static int j
		{
			get
			{
				lock (locker)
				{
					return _j;
				}
			}
			set
			{
				lock (locker)
				{
					_j = value;
				}
			}
		}
	}
}
