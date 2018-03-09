using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using duduluu.System.Linq;

using RimTrans.Core;

namespace Test
{
    class Test
    {
        static void Main(string[] args)
        {
            var totalTime = 0.0;
            for (int i = 0; i < 32; i++) {
                totalTime += TestLoadMods(i);
            }
            Console.WriteLine($"[Total] Elapsed time for loading mods: Total: {totalTime} ms, Average: {totalTime / 32} ms");
        }

        static double TestLoadMods(int index) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Directory.GetDirectories(@"D:\rw\test")
                .ForEach(dir => {
                    var mod = new Mod(dir);
                    mod.LoadDefsAsync().Wait();
                });

            stopwatch.Stop();
            Console.WriteLine($"[{index}] Elapsed time for loading mods: {stopwatch.Elapsed.TotalMilliseconds} ms");

            return stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
