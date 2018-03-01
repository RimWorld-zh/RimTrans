using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RimTrans.Core;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Run().Wait();
        }

        static async Task Run() {
            var mod = new Mod("D:/Games/SteamLibrary/steamapps/common/RimWorld/Mods/Core");
            await mod.LoadDefsAsync();
            //foreach (var def in from submap in mod.defsMap.Values
            //                    from def in submap.Values
            //                    select def) {
            //    Console.WriteLine("{0} {1}", def.filename, def.name);
            //}
        }
    }
}
