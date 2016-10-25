using System;
using System.Collections.Generic;
using System.Linq;
using RimTrans;

namespace RimTrans.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            {
                string modPath = @"D:\Git\RWMod\RimworldAllowTool\Mods\AllowTool";
                Option.ModInfo info = new Option.ModInfo(modPath);
                if (info.IsFolderMeetFomat == false)
                {
                    info.FomatFolderName();
                }

                Mod mod = new Mod(info);
                mod.Generate();
                Console.WriteLine("DefInjectedNew");
                foreach (var kvp in mod.DefInjectedNew)
                {
                    Console.WriteLine(kvp.Key);
                }
                Console.WriteLine("KeyedNew");
                foreach (var kvp in mod.KeyedNew)
                {
                    Console.WriteLine(kvp.Key);
                }
                mod.Export();
            }


            //Mod core = new Mod();
            //foreach (var modInfo in Config.GetModInfos())
            //{
            //    Mod mod = new Mod(modInfo, core);
            //    mod.Generate();
            //    mod.Export();
            //}
        }
    }
}
