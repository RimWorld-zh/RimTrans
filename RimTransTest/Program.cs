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
            // Test Core.
            {
                Console.WriteLine(SteamX.GetDirModsWorkshop());
                Console.WriteLine(SteamX.GetDirRimWorld());
                Console.WriteLine("========");

                Config.TargetLanguage = "Elvish";

                string pathCustom = @"D:\A15Translating\Rimworld\Mods\Core";
                string outputCustom = @"D:\Git\Ludeon\Test";
                Option.ModInfo modInfo = new Option.ModInfo(pathCustom, outputCustom);

                modInfo._Debug();
                Console.WriteLine("========");

                if (modInfo.IsFolderFomatWell == false)
                {
                    modInfo.FomatFolderName();
                }


                DateTime timeStart = DateTime.Now;

                DateTime timeCurrent = DateTime.Now;
                Mod mod = new Mod(modInfo);
                Console.WriteLine("Loaded time: {0}", DateTime.Now - timeCurrent);

                timeCurrent = DateTime.Now;
                mod.Generate();
                Console.WriteLine("Generated time: {0}", DateTime.Now - timeCurrent);

                timeCurrent = DateTime.Now;
                mod.Export();
                Console.WriteLine("Exported time: {0}", DateTime.Now - timeCurrent);

                timeCurrent = DateTime.Now;
                Console.WriteLine("Total time cost: {0}", timeCurrent - timeStart);

                int countInjectionFields = 0;
                int countKeysion = 0;
                Console.WriteLine("==== DefInjected ====");
                foreach (var kvp in mod.DefInjectedNew)
                {
                    int count = kvp.Value.Root.Elements().Count();
                    Console.Write(kvp.Key);
                    Console.Write("  ");
                    Console.WriteLine(count);
                    countInjectionFields += count;
                }
                Console.WriteLine("==== Keyed ====");
                foreach (var kvp in mod.KeyedNew)
                {
                    int count = kvp.Value.Root.Elements().Count();
                    Console.Write(kvp.Key);
                    Console.Write("  ");
                    Console.WriteLine(count);
                    countKeysion += count;
                }
                Console.WriteLine(string.Format("==== DefInjected: {0} === Keyed: {1} ====", countInjectionFields, countKeysion));
            }

            // Test Mod
            {
                //Config.DirRimWorld = @"D:\A15Translating\Rimworld";
                //Config.DirModsWorkshop = @"D:\Game\Steam\steamapps\workshop\content\294100";
                //Console.WriteLine(Config.RimWorldVersion);
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
}
