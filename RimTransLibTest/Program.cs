using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimTransLib;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RimTransLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test();

            //TestMods();

            TestCore();
        }

        public static void LogMessage(object sender, TransLog.MessageArgs e)
        {
            Console.WriteLine(e.Type.ToString() + ": " + e.Title);
            Console.WriteLine(e.Detail);
        }

        public static void Test()
        {
            Console.WriteLine("Test RimTransLib");

            //ModInfo modInfo = new ModInfo(@"D:\Game\RimWorld\Mods\Core");
            //List<LanguageInfo> languageInfos = new List<LanguageInfo>();
            //LanguageInfo languageInfo = new LanguageInfo("Evlish", "Evlish");
            //languageInfos.Add(languageInfo);
            //ModData mod = new ModData(modInfo, languageInfos, true);
            //mod.BuildLanguageData(true);
            //mod.Definition.Display(@"D:\Game\RimWorld\Mods\Core\Defs\ThingDefs_Items\Items_Artifacts.xml");
            //mod.Definition.Display(@"D:\Game\RimWorld\Mods\Core\Defs\ThingDefs_Races\Races_Animal_Arid.xml");
            //mod.Definition.Display(@"D:\Game\RimWorld\Mods\Core\Defs\PawnRelationDefs\PawnRelations_Misc.xml");
            //mod.Definition.Display(@"D:\Game\RimWorld\Mods\Core\Defs\Scenarios\Scenarios_Classic.xml");
            //mod.Definition.Display(@"D:\Game\RimWorld\Mods\Core\Defs\SkillDefs\Skills.xml");
            //mod.Definition.Display(@"D:\Game\RimWorld\Mods\Core\Defs\ThingDefs_Items\Items_Resource_Stuff.xml");
            //mod.LanguageOriginal.Injection.Display("RecipeDef", "Recipes_Add_Make.xml");
            //mod.LanguageOriginal.Injection.Display("RecipeDef", "Recipes_Add_Administer.xml");
            //mod.LanguageOriginal.Injection.Display("ThingDef", "Races_Animal_Arid.xml");
            //mod.LanguageOriginal.Injection.Display("ThingDef", "Buildings_Art.xml");
            //mod.LanguageOriginal.Injection.Display("ThingDef", "Buildings_Furniture.xml");
            //mod.LanguageOriginal.Injection.Display("PawnKindDef", "Races_Animal_Arid.xml");
            //mod.LanguageOriginal.Injection.Display("PawnKindDef", "PawnKinds_Mercenary.xml");
            //mod.LanguageOriginal.Injection.Display("ThingDef", "Races_Mechanoid.xml");

            //ModInfo coreInfo = new ModInfo(@"D:\Game\RimWorld\Mods\Core");
            //List<LanguageInfo> languageInfos = new List<LanguageInfo>();

            //languageInfos.Add(new LanguageInfo("ChineseSimplified", "简体中文", @"D:\Git\RWMod\RimWorld-ChineseSimplified"));
            //ModData core = new ModData(coreInfo, languageInfos, true);
            //core.BuildLanguageData();

            //languageInfos.Add(new LanguageInfo("English", "English", @"D:\Git\RWMod\RimWorld-English"));
            //ModData core = new ModData(coreInfo, languageInfos, true);
            //core.BuildLanguageData(true);
        }

        public static void TestMods()
        {
            TransOption.Initial(@"D:\Game\RimWorld\Mods\Core");
            foreach (LanguageData lang in TransOption.Core.LanguagesExisting)
            {
                Console.WriteLine(lang.LanguageInfo);
            }

            Console.WriteLine("================ Start Building ================");
            
            List<LanguageInfo> languageInfos = new List<LanguageInfo>();
            languageInfos.Add(new LanguageInfo("ChineseSimplified", "简体中文"));
            languageInfos.Add(new LanguageInfo("ChineseTraditional", "繁体中文"));

            DirectoryInfo test = new DirectoryInfo(@"D:\Git\duduluu\RimTrans\test");
            int count = 0;
            foreach (DirectoryInfo modDir in test.GetDirectories())
            {
                Console.WriteLine("================ {0} ================", modDir.Name);
                ModInfo modInfo = new ModInfo(modDir.FullName);
                ModData mod = new ModData(modInfo, languageInfos, false, TransOption.Core);
                mod.BuildLanguageData();
                Console.WriteLine("================ Finished ================");
                count++;
            }
            Console.WriteLine("Finished build language data: {0} mod(s).", count);
        }

        public static void TestCore()
        {
            TransOption.Initial(@"D:\Game\RimWorld\Mods\Core");
            TransOption.Core.BuildLanguageData();

            TransLog.MessageEventHandler += LogMessage;
            Console.WriteLine("================ Start Testing ================");

            List<LanguageInfo> languageInfos = new List<LanguageInfo>();
            languageInfos.AddRange(TransOption.SupportLanguages);
            ModInfo coreInfo = new ModInfo(@"D:\Game\RimWorld\Mods\Core");

            DateTime timeEarly = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                ModData core = new ModData(coreInfo, languageInfos, true);
                core.BuildLanguageData();
            }
            DateTime timeLate = DateTime.Now;

            Console.WriteLine("================ Finished Testing ================");
            Console.WriteLine("{0} languages, {1} times", TransOption.SupportLanguages.Count(), 100);
            Console.WriteLine("Time consuming: ", timeLate - timeEarly);
        }
    }
}
