using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using duduluu.System.Linq;

using RimTrans.Core;

namespace Test {
    class Program {
        static void Main(string[] args) {
            Test(TestLoadMods);
        }

        static void Test(Action testFunc, int frequency = 32) {
            var stopwatch = new Stopwatch();
            var totalTime = 0.0;

            for (int i = 0; i < frequency; i++) {
                stopwatch.Restart();
                testFunc();
                stopwatch.Stop();
                Console.WriteLine($"[# {i}] Elapsed time for {testFunc.Method.Name}: {stopwatch.Elapsed.TotalMilliseconds} ms");
                totalTime += stopwatch.Elapsed.TotalMilliseconds;
            }

            Console.WriteLine($"[Total] Elapsed time for {testFunc.Method.Name}: Total: {totalTime} ms, Average: {totalTime / frequency} ms");
        }

        static void TestLoadMods() {
            Directory.GetDirectories(@"D:\rw\test")
                .ForEach(dir => {
                    var mod = Mod.Load(dir);
                });
        }
    }
}
