using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimTransLib;
using System.IO;

namespace RimTransLibTest
{
    class Program
    {
        static void Main(string[] args)
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

            ModInfo coreInfo = new ModInfo(@"D:\Game\RimWorld\Mods\Core");
            List<LanguageInfo> languageInfos = new List<LanguageInfo>();

            //languageInfos.Add(new LanguageInfo("ChineseSimplified", "简体中文", @"D:\Git\RWMod\RimWorld-ChineseSimplified"));
            //ModData core = new ModData(coreInfo, languageInfos, true);
            //core.BuildLanguageData();

            languageInfos.Add(new LanguageInfo("English", "English", @"D:\Git\RWMod\RimWorld-English"));
            ModData core = new ModData(coreInfo, languageInfos, true);
            core.BuildLanguageData(true);


        }
    }
}
