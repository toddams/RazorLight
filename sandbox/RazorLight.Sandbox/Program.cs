using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using RazorLight.Extensions;
using System.Threading;
using System.Collections.Generic;

namespace RazorLight.Sandbox
{
    class Program
    {
        static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
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
                lock(locker)
                {
                    _j = value;
                }
            }
        }

        private static async Task MainAsync()
        {
            var engine = new RazorLightEngineBuilder()
                .UseMemoryCachingProvider()
                .Build();

            List<string> results = new List<string>();

            for (int i = 0; i < 100; i++)
            {
                ThreadPool.QueueUserWorkItem(async (s) =>
                {
                    try
                    {
                        results.Add(await engine.CompileRenderAsync("Views.Subfolder.go", null, null, null));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("J = " + j + " ============================\n" + e.StackTrace);
                    }

                    j++;
                });
            }

            while(j < 100)
            {
                Console.WriteLine("Waiting: " + j);
                Thread.Sleep(100);
            }

            Console.WriteLine("Finished");
        }
    }
}
