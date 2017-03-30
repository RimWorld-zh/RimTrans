using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RimTrans.Builder;

namespace RimTrans.Trans
{
    class Program
    {
        #region Face Text

        static Random random = new Random();

        static string[] faceTextGood = {
            "╭(￣▽￣)╯   ",
            "(●′?｀●)    ",
            "o(>ω<)o     ",
            " (*ﾟ∇ﾟ)     ",
            " (*´∀`)     ",
            " ( ﾟ∀ﾟ)     ",
            " (￣∇￣)    ",
            "(`・ω・´)   ",
            "(′；ω；‘)   ",
            "(^・ω・^ )  ",
            "╭(●｀?′●)╯  ",
            "(=^･ω･^=)   ",
            "o(*≥▽≤)ツ   ",
            "o(ノﾟ∀ﾟ)ノ  ",
            "(ノ≧∇≦)ノ ",
            "(=^･ｪ･^=)   ",
        };
        public static string FaceGood()
        {
            return faceTextGood[random.Next(9)];
        }

        static string[] faceTextBad =
        {
            "(ﾟДﾟ≡ﾟдﾟ)!? ",
            " ( ;´Д`)    ",
            "(*゜ロ゜)ノ ",
            "Σ( ￣д￣；) ",
            "(っ °Д °;)っ",
            "Σ( ° △ °|||)",
            " (╯°Д°)╯    ",
            "（＞д＜）   ",
            "（ ＴДＴ）  ",
            "(´Ａ｀。)   ",
            "o(￣ヘ￣o＃)",
            "ヽ(#`Д´)ﾉ   ",
            " (|||ﾟдﾟ)   ",
            " ヽ(≧Д≦)ノ",
            " ヽ(#`Д´)ﾉ  ",
            "_(:з」∠)_  ",
        };
        public static string FaceBad()
        {
            return faceTextBad[random.Next(9)];
        }
        
        #endregion

        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            #region Application Info

            Assembly asm = Assembly.GetExecutingAssembly();
            string title = asm.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            string version = asm.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            string copyright = asm.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

            Console.Title = title;
            Console.OutputEncoding = Encoding.Unicode;
            Log.WriteLine();
            Log.WriteLine(ConsoleColor.Cyan, $" {FaceGood()}{FaceGood()}{FaceGood()}");
            Log.WriteLine();
            Log.WriteLine(ConsoleColor.Cyan, $" {title}");
            Log.WriteLine();
            Log.WriteLine(ConsoleColor.Cyan, $" Version {version}");
            Log.WriteLine();
            Log.WriteLine(ConsoleColor.Cyan, $" {copyright}");
            Log.WriteLine();

            #endregion

            #region Check  Arguments

            string projectFile = null;
            string corePath = null;
            bool cleanModeOn = false;
            string modPath = null;
            string generateOption = null;

            foreach (string argument in args)
            {
                if (argument.StartsWith("-p:") && argument.Length > 3)
                {
                    projectFile = argument.Substring(3);
                }
                else if (argument.StartsWith("-Core:") && argument.Length > 6)
                {
                    corePath = argument.Substring(6);
                }
                else if (argument == "-Clean")
                {
                    cleanModeOn = true;
                }
            }

            // Check Project File
            if (string.IsNullOrWhiteSpace(projectFile) || !File.Exists(projectFile))
            {
                Log.Error();
                Log.WriteLine(ConsoleColor.Red, $"Project File {projectFile} NO FOUND.");
                Log.WriteLine();
                Console.Write("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Log.WriteLine(ConsoleColor.Green, $"======== Start Project  {FaceGood()} ========");
            Log.WriteLine();
            Log.WriteLine(ConsoleColor.Green, "Porject File: ");
            Log.Indent();
            Log.WriteLine(ConsoleColor.Cyan, projectFile);

            XDocument doc = XDocument.Load(projectFile);
            XElement root = doc.Root;
            modPath = root.Element("ModPath").Value;

            // Check Mod Path
            if (string.IsNullOrWhiteSpace(modPath) || !Directory.Exists(modPath))
            {
                Log.Error();
                Log.WriteLine(ConsoleColor.Red, $"Mod Directory {modPath} NO FOUND.");
                Log.WriteLine();
                Console.Write("Press any key to exit...");
                Console.ReadKey();
                return;
            }
            
            Log.WriteLine(ConsoleColor.Green, "Mod Path: ");
            Log.Indent();
            Log.WriteLine(ConsoleColor.Cyan, modPath);

            generateOption = root.Element("GenerateOption").Value;
            Log.WriteLine(ConsoleColor.Green, "Generate Option: ");
            Log.Indent();
            Log.Write(ConsoleColor.Cyan, generateOption + " Mode");
            if (cleanModeOn)
            {
                Log.WriteLine(ConsoleColor.Green, " | Clean Mode");
            }
            else
            {
                Log.WriteLine();
            }

            // Check Core Path
            if (generateOption == "Standard" &&
                (string.IsNullOrWhiteSpace(corePath) || !Directory.Exists(corePath)))
            {
                generateOption = "Core";
                Log.Warning();
                Log.WriteLine(ConsoleColor.Yellow, "Core Directory NO FOUND.");
                Log.Indent();
                Log.Write(ConsoleColor.Yellow, "Changed Generate Option to ");
                Log.WriteLine(ConsoleColor.Cyan, "Core Mode");
            }

            if (generateOption == "Standard")
            {
                Log.WriteLine(ConsoleColor.Green, "Core Path: ");
                Log.Indent();
                Log.WriteLine(ConsoleColor.Cyan, corePath);
            }

            Log.WriteLine();

            #endregion

            #region Process Core and Special Mode

            if (generateOption == "Core")
            {

                Log.WriteLine(ConsoleColor.Green, "======== Start Processing Defs and Original Language Data ========");
                string defsPath = Path.Combine(modPath, "Defs");
                Console.WriteLine(defsPath);
                string keyedPath_English = Path.Combine(modPath, "Languages", "English", "Keyed");
                string stringsPath_English = Path.Combine(modPath, "Languages", "English", "Strings");
                DefinitionData Defs = DefinitionData.Load(defsPath);
                InjectionData DefInjected_Original = InjectionData.Parse(Defs);
                KeyedData Keyed_English = KeyedData.Load(keyedPath_English);
                Log.WriteLine(ConsoleColor.Green, "======== Completed Processing Defs and Original Language Data ========");
                Log.WriteLine();

                XElement Languages = root.Element("Languages");
                foreach (XElement li in Languages.Elements())
                {
                    // If IsChecked
                    if (string.Compare(li.Element("IsChecked").Value, "false", true) == 0)
                        continue;

                    string realName = li.Element("RealName").Value;
                    string nativeName = li.Element("NativeName").Value;

                    // Check Language Real Name
                    if (string.IsNullOrWhiteSpace(realName))
                    {
                        Log.Error();
                        Log.WriteLine(ConsoleColor.Red, "Missing Language Name.");
                        continue;
                    }

                    Log.WriteLine(ConsoleColor.Green, $"======== Start Processing Language: {realName} ( {nativeName} ) ========");

                    string langPath = Path.Combine(modPath, "Languages", realName);
                    bool isCustom = (string.Compare(li.Element("IsCustom").Value, "true", true) == 0);
                    if (isCustom)
                    {
                        langPath = li.Element("CustomPath").Value;
                        Log.WriteLine(ConsoleColor.Cyan, "Use Custom Language Output Directory: ");
                    }
                    else
                    {
                        Log.WriteLine(ConsoleColor.Cyan, "Language Path: ");
                    }
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Cyan, langPath);

                    string defInjectedPath = Path.Combine(langPath, "DefInjected");
                    string keyedPath = Path.Combine(langPath, "Keyed");
                    string stringsPath = Path.Combine(langPath, "Strings");

                    InjectionData DefInjected_New = new InjectionData(DefInjected_Original);
                    KeyedData Keyed_New = new KeyedData(Keyed_English);

                    if (cleanModeOn)
                    {
                        DirectoryHelper.CleanDirectory(defInjectedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(keyedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(stringsPath, "*.txt");
                    }
                    else
                    {
                        InjectionData DefInjected_Existed = InjectionData.Load(defInjectedPath, true);
                        DefInjected_New.MatchExisted(DefInjected_Existed);

                        KeyedData Keyed_Existed = KeyedData.Load(keyedPath, true);
                        Keyed_New.MatchExisted(Keyed_Existed);
                    }

                    DefInjected_New.Save(defInjectedPath);
                    Keyed_New.Save(keyedPath);
                    DirectoryHelper.CopyDirectoryEx(stringsPath_English, stringsPath, "*.txt");

                    Log.WriteLine(ConsoleColor.Green, $"======== Completed Processing Language: {realName} ( {nativeName} ) ========");
                    Log.WriteLine();
                }
            }

            #endregion

            #region Process Standard Mode

            else if (generateOption == "Standard")
            {

                Log.WriteLine(ConsoleColor.Green, "======== Start Processing Core Defs and Original Language Data ========");
                string core_defsPath = Path.Combine(corePath, "Defs");
                string core_langPath = Path.Combine(corePath, "Languages");
                string core_keyedPath_English = Path.Combine(core_langPath, "English", "Keyed");

                DefinitionData Core_Defs = DefinitionData.Load(core_defsPath);
                InjectionData Core_DefInjected_Original = InjectionData.Parse(Core_Defs);
                KeyedData Core_Keyed_English = KeyedData.Load(core_keyedPath_English);
                Log.WriteLine(ConsoleColor.Green, "======== Completed Processing Core Defs and Original Language Data ========");
                Log.WriteLine();
                
                Log.WriteLine(ConsoleColor.Green, "======== Start Processing Mod Defs and Original Language Data ========");
                string defsPath = Path.Combine(modPath, "Defs");
                string keyedPath_English = Path.Combine(modPath, "Languages", "English", "Keyed");
                string stringsPath_English = Path.Combine(modPath, "Languages", "English", "Strings");

                DefinitionData Defs = DefinitionData.Load(defsPath, Core_Defs);
                InjectionData DefInjected_Original = InjectionData.Parse(Defs);
                KeyedData Keyed_English = KeyedData.Load(keyedPath_English);
                Log.WriteLine(ConsoleColor.Green, "======== Completed Processing Mod Defs and Original Language Data ========");
                Log.WriteLine();

                XElement Languages = root.Element("Languages");
                foreach (XElement li in Languages.Elements())
                {
                    // If IsChecked
                    if (string.Compare(li.Element("IsChecked").Value, "false", true) == 0)
                        continue;

                    string realName = li.Element("RealName").Value;
                    string nativeName = li.Element("NativeName").Value;

                    // Check Language Real Name
                    if (string.IsNullOrWhiteSpace(realName))
                    {
                        Log.Error();
                        Log.WriteLine(ConsoleColor.Red, "Missing Language Name.");
                        continue;
                    }

                    Log.WriteLine(ConsoleColor.Green, $"======== Start Processing Language: {realName} ( {nativeName} ) ========");

                    string langPath = Path.Combine(modPath, "Languages", realName);
                    bool isCustom = (string.Compare(li.Element("IsCustom").Value, "true", true) == 0);
                    if (isCustom)
                    {
                        langPath = li.Element("CustomPath").Value;
                        Log.WriteLine(ConsoleColor.Cyan, "Use Custom Language Output Directory: ");
                    }
                    else
                    {
                        Log.WriteLine(ConsoleColor.Cyan, "Language Path: ");
                    }
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Cyan, langPath);

                    // Check Custom Path
                    if (string.IsNullOrWhiteSpace(langPath))
                    {
                        Log.Error();
                        Log.WriteLine(ConsoleColor.Red, "Invalid Custom Output Directory Path.");
                        continue;
                    }

                    string core_defInjectedPath = Path.Combine(core_langPath, realName, "DefInjected");
                    string core_keyedPath = Path.Combine(core_langPath, realName, "Keyed");

                    InjectionData Core_DefInjected_New = new InjectionData(Core_DefInjected_Original);
                    InjectionData Core_DefInjected_Existed = InjectionData.Load(core_defInjectedPath);
                    Core_DefInjected_New.MatchExisted(Core_DefInjected_Existed);

                    KeyedData Core_Keyed_New = new KeyedData(Core_Keyed_English);
                    KeyedData Core_Keyed_Existed = KeyedData.Load(core_keyedPath);
                    Core_Keyed_New.MatchExisted(Core_Keyed_Existed);

                    string defInjectedPath = Path.Combine(langPath, "DefInjected");
                    string keyedPath = Path.Combine(langPath, "Keyed");
                    string stringsPath = Path.Combine(langPath, "Strings");

                    InjectionData DefInjected_New = new InjectionData(DefInjected_Original);
                    DefInjected_New.MatchCore(Core_DefInjected_New);

                    KeyedData Keyed_New = new KeyedData(Keyed_English);
                    Keyed_New.MatchCore(Core_Keyed_New);

                    if (cleanModeOn)
                    {
                        DirectoryHelper.CleanDirectory(defInjectedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(keyedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(stringsPath, "*.txt");
                    }
                    else
                    {
                        InjectionData DefInjected_Existed = InjectionData.Load(defInjectedPath, true);
                        DefInjected_New.MatchExisted(DefInjected_Existed);

                        KeyedData Keyed_Existed = KeyedData.Load(keyedPath, true);
                        Keyed_New.MatchExisted(Keyed_Existed);
                    }

                    DefInjected_New.Save(defInjectedPath);
                    Keyed_New.Save(keyedPath);
                    DirectoryHelper.CopyDirectoryEx(stringsPath_English, stringsPath, "*.txt");

                    Log.WriteLine(ConsoleColor.Green, $"======== Completed Processing Language: {realName} ( {nativeName} ) ========");
                    Log.WriteLine();
                }
            }

            #endregion

            #region Special Mode

            else if (generateOption == "Special")
            {

                Log.WriteLine(ConsoleColor.Green, "======== Start Processing Core Defs and Original Language Data ========");
                string core_defsPath = Path.Combine(corePath, "Defs");
                string core_langPath = Path.Combine(corePath, "Languages");
                string core_keyedPath_English = Path.Combine(core_langPath, "English", "Keyed");

                DefinitionData Core_Defs = DefinitionData.Load(core_defsPath);
                //InjectionData Core_DefInjected_Original = InjectionData.Parse(Core_Defs);
                //KeyedData Core_Keyed_English = KeyedData.Load(core_keyedPath_English);
                Log.WriteLine(ConsoleColor.Green, "======== Completed Processing Core Defs ========");
                Log.WriteLine();

                Log.WriteLine(ConsoleColor.Green, "======== Start Processing Mod Defs and Original Language Data ========");
                string defsPath = Path.Combine(modPath, "Defs");
                string keyedPath_English = Path.Combine(modPath, "Languages", "English", "Keyed");
                string stringsPath_English = Path.Combine(modPath, "Languages", "English", "Strings");

                DefinitionData Defs = DefinitionData.Load(defsPath, Core_Defs);
                InjectionData DefInjected_Original = InjectionData.Parse(Defs);
                KeyedData Keyed_English = KeyedData.Load(keyedPath_English);
                Log.WriteLine(ConsoleColor.Green, "======== Completed Processing Mod Defs and Original Language Data ========");
                Log.WriteLine();

                XElement Languages = root.Element("Languages");
                foreach (XElement li in Languages.Elements())
                {
                    // If IsChecked
                    if (string.Compare(li.Element("IsChecked").Value, "false", true) == 0)
                        continue;

                    string realName = li.Element("RealName").Value;
                    string nativeName = li.Element("NativeName").Value;

                    // Check Language Real Name
                    if (string.IsNullOrWhiteSpace(realName))
                    {
                        Log.Error();
                        Log.WriteLine(ConsoleColor.Red, "Missing Language Name.");
                        continue;
                    }

                    Log.WriteLine(ConsoleColor.Green, $"======== Start Processing Language: {realName} ( {nativeName} ) ========");

                    string langPath = Path.Combine(modPath, "Languages", realName);
                    bool isCustom = (string.Compare(li.Element("IsCustom").Value, "true", true) == 0);
                    if (isCustom)
                    {
                        langPath = li.Element("CustomPath").Value;
                        Log.WriteLine(ConsoleColor.Cyan, "Use Custom Language Output Directory: ");
                    }
                    else
                    {
                        Log.WriteLine(ConsoleColor.Cyan, "Language Path: ");
                    }
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Cyan, langPath);

                    // Check Custom Path
                    if (string.IsNullOrWhiteSpace(langPath))
                    {
                        Log.Error();
                        Log.WriteLine(ConsoleColor.Red, "Invalid Custom Output Directory Path.");
                        continue;
                    }

                    string core_defInjectedPath = Path.Combine(core_langPath, realName, "DefInjected");
                    string core_keyedPath = Path.Combine(core_langPath, realName, "Keyed");

                    //InjectionData Core_DefInjected_New = new InjectionData(Core_DefInjected_Original);
                    //InjectionData Core_DefInjected_Existed = InjectionData.Load(core_defInjectedPath);
                    //Core_DefInjected_New.MatchExisted(Core_DefInjected_Existed);

                    //KeyedData Core_Keyed_New = new KeyedData(Core_Keyed_English);
                    //KeyedData Core_Keyed_Existed = KeyedData.Load(core_keyedPath);
                    //Core_Keyed_New.MatchExisted(Core_Keyed_Existed);

                    string defInjectedPath = Path.Combine(langPath, "DefInjected");
                    string keyedPath = Path.Combine(langPath, "Keyed");
                    string stringsPath = Path.Combine(langPath, "Strings");

                    InjectionData DefInjected_New = new InjectionData(DefInjected_Original);
                    //DefInjected_New.MatchCore(Core_DefInjected_New);

                    KeyedData Keyed_New = new KeyedData(Keyed_English);
                    //Keyed_New.MatchCore(Core_Keyed_New);

                    if (cleanModeOn)
                    {
                        DirectoryHelper.CleanDirectory(defInjectedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(keyedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(stringsPath, "*.txt");
                    }
                    else
                    {
                        InjectionData DefInjected_Existed = InjectionData.Load(defInjectedPath, true);
                        DefInjected_New.MatchExisted(DefInjected_Existed);

                        KeyedData Keyed_Existed = KeyedData.Load(keyedPath, true);
                        Keyed_New.MatchExisted(Keyed_Existed);
                    }

                    DefInjected_New.Save(defInjectedPath);
                    Keyed_New.Save(keyedPath);
                    DirectoryHelper.CopyDirectoryEx(stringsPath_English, stringsPath, "*.txt");

                    Log.WriteLine(ConsoleColor.Green, $"======== Completed Processing Language: {realName} ( {nativeName} ) ========");
                    Log.WriteLine();
                }
            }

            #endregion

            // End
            Log.WriteLine(ConsoleColor.Green, $"======== Completed Project  {FaceGood()}========");
            Log.WriteLine();
            Console.Write("Press any key to exit...");
            Console.ReadKey();
            return;
        }
    }
}
