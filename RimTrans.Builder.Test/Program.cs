using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using RimTrans.Builder;
using RimTrans.Builder.Xml;

namespace RimTransLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test_Core();
            //Test_Core_Load_All();
            //Test_VG();
            //Test_294100();
        }

        #region Full Test

        static void Test_Core()
        {
            string pathCore = @"D:\Test\Core";
            string pathDefs = Path.Combine(pathCore, "Defs");
            string pathLanguage = Path.Combine(pathCore, "Languages");

            string pathEnglish = Path.Combine(pathLanguage, "English");
            string pathEnglishKeyed = Path.Combine(pathEnglish, "Keyed");
            string pathEnglishStrings = Path.Combine(pathEnglish, "Strings");

            string pathChinese = Path.Combine(pathLanguage, "ChineseSimplified");
            string pathChineseDefInjected = Path.Combine(pathChinese, "DefInjected");
            string pathChineseKeyed = Path.Combine(pathChinese, "Keyed");
            string pathChineseStrings = Path.Combine(pathChinese, "Strings");

            string pathEvlish = @"D:\Git\RWMod\RimWorld-English"; // Path.Combine(pathLanguage, "Evlish");
            string pathEvlishDefInjected = Path.Combine(pathEvlish, "DefInjected");
            string pathEvlishKeyed = Path.Combine(pathEvlish, "Keyed");
            string pathEvlishStrings = Path.Combine(pathEvlish, "Strings");

            Log.WriteLine(ConsoleColor.Magenta, "================ Load Defs ================");
            DefinitionData Defs = DefinitionData.Load(pathDefs);
            Log.WriteLine(ConsoleColor.Magenta, "================ Generate Original DefInjected ================");
            InjectionData DefInjected_Original = InjectionData.Parse(Defs);
            Log.WriteLine(ConsoleColor.Magenta, "================ Load Original Keyed ================");
            KeyedData Keyed_Original = KeyedData.Load(pathEnglishKeyed);

            //Log.WriteLine();

            Log.WriteLine(ConsoleColor.Magenta, "================ Create Chinese DefInjected ================");
            InjectionData DefInjected_CS_Existed = InjectionData.Load(pathChineseDefInjected, true);
            InjectionData DefInjected_CS_New = new InjectionData(DefInjected_Original);
            DefInjected_CS_New.MatchExisted(DefInjected_CS_Existed);
            DefInjected_CS_New.Save(pathChineseDefInjected);
            Log.WriteLine(ConsoleColor.Magenta, "================ Create Chinese Keyed ================");
            KeyedData Keyed_CS_Existed = KeyedData.Load(pathChineseKeyed, true);
            KeyedData Keyed_CS_New = new KeyedData(Keyed_Original);
            Keyed_CS_New.MatchExisted(Keyed_CS_Existed);
            Keyed_CS_New.Save(pathChineseKeyed);
            Log.WriteLine(ConsoleColor.Magenta, "================ Create Chinese Strings ================");
            DirectoryHelper.CopyDirectoryEx(pathEnglishStrings, pathChineseStrings, "*.txt");

            //Log.WriteLine();

            Log.WriteLine(ConsoleColor.Magenta, "================ Clean Evlish ================");
            DirectoryHelper.CleanDirectory(pathEvlishDefInjected, "*.xml");
            DirectoryHelper.CleanDirectory(pathEvlishKeyed, "*.xml");
            DirectoryHelper.CleanDirectory(pathEvlishStrings, "*.txt");
            Log.WriteLine(ConsoleColor.Magenta, "================ Create Evlish DefInjected ================");
            DefInjected_Original.Save(pathEvlishDefInjected);
            Log.WriteLine(ConsoleColor.Magenta, "================ Create Evlish Keyed ================");
            Keyed_Original.Save(pathEvlishKeyed);
            Log.WriteLine(ConsoleColor.Magenta, "================ Create Evlish Strings ================");
            DirectoryHelper.CopyDirectoryEx(pathEnglishStrings, pathEvlishStrings, "*.txt");
        }

        static void Test_Core_Load_All()
        {
            DirectoryInfo dirInfoLanguages = new DirectoryInfo(@"D:\Test\Core\Languages");
            foreach (DirectoryInfo lang in dirInfoLanguages.GetDirectories())
            {
                Log.WriteLine(ConsoleColor.Magenta, "================ {0} ================", lang.Name);
                InjectionData DefInjected = InjectionData.Load(Path.Combine(lang.FullName, "DefInjected"));
                KeyedData Keyed = KeyedData.Load(Path.Combine(lang.FullName, "Keyed"));
            }
        }

        static void Test_VG()
        {
            Log.WriteLine(ConsoleColor.Magenta, "================ Load Core ================");
            DefinitionData CoreDefs = DefinitionData.Load(@"D:\Test\Core\Defs");
            InjectionData CoreDefInjectedChinese = InjectionData.Load(@"D:\Test\Core\Languages\ChineseSimplified\DefInjected");
            Log.WriteLine(ConsoleColor.Magenta, " ================ Vegetable Garden ================");
            DefinitionData VgDefs = DefinitionData.Load(@"D:\Test\Vegetable Garden 5.3\Defs", CoreDefs);
            InjectionData VgDefInjectedOrignial = InjectionData.Parse(VgDefs);
            InjectionData VgDefInjectedExisted = InjectionData.Load(@"D:\Test\Vegetable Garden 5.3\Languages\ChineseSimplified\DefInjected");
            InjectionData VgDefInjectedNew = new InjectionData(VgDefInjectedOrignial);
            VgDefInjectedNew.MatchCore(CoreDefInjectedChinese);
            VgDefInjectedNew.MatchExisted(VgDefInjectedExisted);
            VgDefInjectedNew.Save(@"D:\Test\Vegetable Garden 5.3\Languages\ChineseSimplified\DefInjected");
            Log.WriteLine(ConsoleColor.Magenta, "================ Vegetable Garden (without Core) ================");
            VgDefs = DefinitionData.Load(@"D:\Test\Vegetable Garden 5.3\Defs");
            VgDefInjectedOrignial = InjectionData.Parse(VgDefs);
        }

        static void Test_294100()
        {
            DateTime time = DateTime.Now;

            DefinitionData CoreDefs = DefinitionData.Load(@"D:\Test\Core\Defs");
            InjectionData CoreDefInjectedChinese = InjectionData.Load(@"D:\Test\Core\Languages\ChineseSimplified\DefInjected");

            DirectoryInfo dirInfo = new DirectoryInfo(@"D:\Test\294100");
            int count = 0;
            foreach (DirectoryInfo mod in dirInfo.GetDirectories())
            {
                string pathMod = mod.FullName;
                string pathDefs = Path.Combine(pathMod, "Defs");
                string pathChinese = Path.Combine(pathMod, "Languages", "ChineseSimplified");
                string pathEnglish = Path.Combine(pathMod, "Languages", "English");
                string pathDefInjected = Path.Combine(pathChinese, "DefInjected");
                string pathKeyed = Path.Combine(pathChinese, "Keyed");
                string pathKeyedEnglish = Path.Combine(pathEnglish, "Keyed");
                string pathStrings = Path.Combine(pathChinese, "Strings");
                string pathStringsEnglish = Path.Combine(pathEnglish, "Strings");

                if (!Directory.Exists(pathDefs) && !Directory.Exists(pathEnglish))
                {
                    continue;
                }

                //if (!Directory.Exists(pathDefInjected))
                //{
                //    continue;
                //}

                Log.WriteLine(ConsoleColor.Magenta, "================ {0} ================", mod.Name);
                DefinitionData Defs = DefinitionData.Load(pathDefs, CoreDefs);
                InjectionData InjectOrigin = InjectionData.Parse(Defs);
                InjectionData InjectExisted = InjectionData.Load(pathDefInjected, true);
                InjectionData InjectNew = new InjectionData(InjectOrigin);
                InjectNew.MatchCore(CoreDefInjectedChinese);
                InjectNew.MatchExisted(InjectExisted);
                InjectNew.Save(pathDefInjected);
                KeyedData KeyedOrigin = KeyedData.Load(pathKeyedEnglish);
                KeyedData KeyedExisted = KeyedData.Load(pathKeyed, true);
                KeyedData KeyedNew = new KeyedData(KeyedOrigin);
                KeyedNew.MatchExisted(KeyedExisted);
                KeyedNew.Save(pathKeyed);
                DirectoryHelper.CopyDirectoryEx(pathStringsEnglish, pathStrings, "*.txt");
                count++;
            }

            Log.WriteLine(ConsoleColor.Magenta, "================ COMPLETE: {0} mods ================", count);
            Log.WriteLine(ConsoleColor.Magenta, "================ {0} ================", DateTime.Now - time);

        }

        #endregion

        #region Unit Test



        #endregion
    }
}
