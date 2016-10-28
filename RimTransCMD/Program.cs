using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RimTrans.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {


            if (Directory.Exists(Config.DirRimWorld) == false)
            {
                Config.DirRimWorld = SteamX.GetDirRimWorld();
            }
            if (Directory.Exists(Config.DirModsWorkshop) == false)
            {
                Config.DirModsWorkshop = SteamX.GetDirModsWorkshop();
            }
            CommandStart(); 

            while (true)
            {
                Console.Write("RIMTRANS> ");
                string cmdLine = Console.ReadLine();
                if (cmdLine != string.Empty)
                {
                    string command = null;
                    List<Match> arguments = new List<Match>();
                    List<Match> headers = new List<Match>();
                    bool flag = false;
                    foreach (Match match in Regex.Matches(cmdLine, patternArgument))
                    {
                        if (flag)
                        {
                            arguments.Add(match);
                        }
                        else
                        {
                            command = match.Value;
                            flag = true;
                        }
                    }
                    foreach (Match match in Regex.Matches(cmdLine, patternHeader))
                    {
                        headers.Add(match);
                    }

                    if (command != null)
                    {
                        if (string.Compare(command, "exit", true) == 0)
                        {
                            break;
                        }
                        else if (string.Compare(command, "info", true) == 0)
                        {
                            CommandInfo(arguments);
                        }
                        else if (string.Compare(command, "set", true) == 0)
                        {
                            CommandSet(arguments, headers);
                            //DisplayInfo();
                        }
                        else if (string.Compare(command, "trans", true) == 0)
                        {
                            CommandTrans(arguments, headers);
                        }
                        else if (string.Compare(command, "trans-custom", true) == 0)
                        {
                            CommandTransCustom(arguments, headers);
                        }
                        else
                        {
                            Error.UnAvailableCommand(command);
                        }
                    }
                }
            }
        }

        static readonly string patternArgument = "/\\s*(\".+?\"|[^:\\s])+((\\s*:\\s*(\".+?\"|[^\\s])+)|)|(\".+?\"|[^\"\\s])+";
        static readonly string patternHeader = "/\\s*(\".+?\"|[^:\\s])+(?=[:\\s])";
        
        static void CommandStart()
        {
            Console.WriteLine();
            Console.WriteLine("RimTrans CMD [Version {0}]", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine();
            Console.WriteLine("MIT License, Copyright (c) 2016 duduluu");
            Console.WriteLine();
            DisplayInfo();
            Console.WriteLine();

        }

        static void DisplayInfo()
        {
            Console.WriteLine("Global Variables:");
            Console.WriteLine("Game-Dir: {0}", Config.DirRimWorld);
            Console.WriteLine("Workshop-Dir: {0}", Config.DirModsWorkshop);
            Console.WriteLine("Target-Language: {0}", Config.TargetLanguage);
        }

        /// <summary>
        /// Commadn Info
        /// </summary>
        static void CommandInfo(List<Match> arguments)
        {
            if (arguments.Count() == 0)
            {
                DisplayInfo();
            }
            else if (arguments.Count() == 1)
            {
                string argument = arguments.First().Value;
                if (string.Compare(argument, "/all") == 0)
                {
                    Console.WriteLine("All Mods:");
                    foreach (var modInfo in Config.GetModInfos(Option.Where.Direct | Option.Where.Workshop))
                    {
                        Console.WriteLine(modInfo);
                    }
                }
                else if (string.Compare(argument, "/direct") == 0)
                {
                    Console.WriteLine("Direct Mods:");
                    foreach (var modInfo in Config.GetModInfos(Option.Where.Direct))
                    {
                        Console.WriteLine(modInfo);
                    }
                }
                else if (string.Compare(argument, "/workshop") == 0)
                {
                    Console.WriteLine("Workshop Mods:");
                    foreach (var modInfo in Config.GetModInfos(Option.Where.Workshop))
                    {
                        Console.WriteLine(modInfo);
                    }
                }
                else
                {
                    Error.ArgumentsError("Info");
                }
            }
            else
            {
                Error.ArgumentsError("Info");
            }
        }

        /// <summary>
        /// Commadn Set
        /// </summary>
        static void CommandSet(List<Match> arguments, List<Match> headers)
        {
            if (arguments.Count() > 0 && arguments.Count() == headers.Count())
            {
                string gameDir = null;
                string workshopDir = null;
                string targetLanguage = null;

                bool isError = false;
                for (int i = 0; i < arguments.Count() && i < headers.Count(); i++)
                {
                    // Game-Dir
                    if (string.Compare(headers[i].Value, "/game-dir", true) == 0)
                    {
                        string path = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (gameDir == null)
                        {
                            if (Directory.Exists(path) || string.Compare(path, "auto", true) == 0)
                            {
                                gameDir = path;
                            }
                            else
                            {
                                Error.DirectoryNotFound(path);
                            }
                        }
                        else
                        {
                            Error.ConflictArguments(arguments[i].Value);
                            isError = false;
                        }
                    }
                    // Workshop-Dir
                    else if (string.Compare(headers[i].Value, "/workshop-dir", true) == 0)
                    {
                        string path = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (workshopDir == null)
                        {
                            if (Directory.Exists(path) || string.Compare(path, "auto", true) == 0)
                            {
                                workshopDir = path;
                            }
                            else
                            {
                                Error.DirectoryNotFound(path);
                            }
                        }
                        else
                        {
                            Error.ConflictArguments(arguments[i].Value);
                            isError = false;
                        }
                    }
                    // Target-Language
                    else if (string.Compare(headers[i].Value, "/target-language", true) == 0)
                    {
                        string language = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (targetLanguage == null)
                        {
                            if (language.IsValidLanguage())
                            {
                                targetLanguage = language;
                            }
                            else
                            {
                                Error.InvalidTargetLanguage(targetLanguage);
                                isError = true;
                            }
                        }
                        else
                        {
                            Error.ConflictArguments(arguments[i].Value);
                            isError = true;
                        }
                    }
                    else
                    {
                        Error.InvalidArguments(arguments[i].Value);
                        isError = true;
                    }
                }

                if (isError)
                {
                    Error.ArgumentsError("Set");
                }
                else
                {
                    if (gameDir != null)
                    {
                        if (string.Compare(gameDir, "auto", true) == 0)
                        {
                            Config.DirRimWorld = SteamX.GetDirRimWorld();
                        }
                        else
                        {
                            Config.DirRimWorld = gameDir;
                        }
                        Console.WriteLine("Game-Dir set to: {0}", Config.DirRimWorld);
                    }
                    if (workshopDir != null)
                    {
                        if (string.Compare(workshopDir, "auto", true) == 0)
                        {
                            Config.DirModsWorkshop = SteamX.GetDirModsWorkshop();
                        }
                        else
                        {
                            Config.DirModsWorkshop = workshopDir;
                        }
                        Console.WriteLine("Workshop-Dir set to: {0}", Config.DirModsWorkshop);
                    }
                    if (targetLanguage != null)
                    {
                        Config.TargetLanguage = targetLanguage;
                        Console.WriteLine("Target-Language set to: {0}", Config.TargetLanguage);
                    }
                }
            }
            else
            {
                Error.ArgumentsError("Set");
            }
        }

        /// <summary>
        /// Commadn Trans
        /// </summary>
        static void CommandTrans(List<Match> arguments, List<Match> headers)
        {
            if (arguments.Count() > 0 && arguments.Count() == headers.Count())
            {
                string modName = null;
                string lang = null;
                string output = null;
                Option.Where where = Option.Where.None;

                bool isError = false;
                for (int i = 0; i < arguments.Count() && i < headers.Count(); i++)
                {
                    if (string.Compare(headers[i].Value, "/mod") == 0)
                    {
                        string name = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (modName == null)
                        {
                            modName = name;
                        }
                        else
                        {
                            Error.ConflictArguments(arguments[i].Value);
                            isError = true;
                        }
                    }
                    else if (string.Compare(headers[i].Value, "/where") == 0)
                    {
                        string whereValue = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (where == Option.Where.None)
                        {
                            Option.Where temp;
                            if (Enum.TryParse(whereValue, true, out temp) &&
                                (temp == Option.Where.Direct || temp == Option.Where.Workshop))
                            {
                                where = temp;
                            }
                            else
                            {
                                Error.InvalidArguments(arguments[i].Value);
                                isError = true;
                            }
                        }
                        else
                        {
                            Error.ConflictArguments(arguments[i].Value);
                            isError = true;
                        }
                    }
                    else if (string.Compare(headers[i].Value, "/lang") == 0)
                    {
                        string language = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (lang == null)
                        {
                            if (language.IsValidLanguage())
                            {
                                lang = language;
                            }
                            else
                            {
                                Error.InvalidTargetLanguage(language);
                                isError = true;
                            }
                        }
                        else
                        {
                            Error.ConflictArguments(arguments[i].Value);
                            isError = true;
                        }
                    }
                    else if (string.Compare(headers[i].Value, "/output") == 0)
                    {
                        string path = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (output == null)
                        {
                            if (path.IsValidPath())
                            {
                                output = path;
                            }
                            else
                            {
                                Error.InvalidPath(path);
                                isError = true;
                            }
                        }
                        else
                        {
                            Error.ConflictArguments(arguments[i].Value);
                            isError = true;
                        }
                    }
                    else
                    {
                        isError = true;
                    }
                }

                if (isError || modName == null)
                {
                    Error.ArgumentsError("Trans");
                }
                else
                {
                    string earlyTargetLanguage = Config.TargetLanguage;
                    if (lang != null) Config.TargetLanguage = lang;
                    if (where == Option.Where.None) where = Option.Where.Direct;
                    Option.ModInfo modInfo = new Option.ModInfo(modName, where, output);
                    if (modInfo.IsValidMod)
                    {
                        Mod mod;
                        if (string.Compare(modName, "core", true) == 0)
                        {
                            mod = new Mod(modInfo);
                        }
                        else
                        {
                            mod = new Mod(modInfo, Mod.Core);
                        }
                        Console.WriteLine("Mod: {0}", modInfo.ModPath);
                        mod.Generate();
                        mod.Export();
                        Console.WriteLine("Output to: {0}", modInfo.TargetLanguage);
                    }
                    else
                    {
                        Error.InvalidMod(modName);
                    }
                    Config.TargetLanguage = earlyTargetLanguage;
                }
            }
            else
            {
                Error.ArgumentsError("Trans");
            }
        }

        /// <summary>
        /// Commadn Trans-Custom
        /// </summary>
        static void CommandTransCustom(List<Match> arguments, List<Match> headers)
        {
            if (arguments.Count() > 0 && arguments.Count() == headers.Count())
            {
                string modPath = null;
                string lang = null;
                string output = null;

                bool isError = false;
                for (int i = 0; i < arguments.Count() && i < headers.Count(); i++)
                {
                    if (string.Compare(headers[i].Value, "/mod-path") == 0)
                    {
                        string path = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (modPath == null)
                        {
                            modPath = path;
                        }
                        else
                        {
                            Error.ConflictArguments(arguments[i].Value);
                            isError = true;
                        }
                    }
                    else if (string.Compare(headers[i].Value, "/lang") == 0)
                    {
                        string language = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (lang == null)
                        {
                            if (language.IsValidLanguage())
                            {
                                lang = language;
                            }
                            else
                            {
                                Error.InvalidTargetLanguage(language);
                                isError = true;
                            }
                        }
                        else
                        {
                            Error.ConflictArguments(arguments[i].Value);
                            isError = true;
                        }
                    }
                    else if (string.Compare(headers[i].Value, "/output") == 0)
                    {
                        string path = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (output == null)
                        {
                            if (path.IsValidPath())
                            {
                                output = path;
                            }
                            else
                            {
                                Error.InvalidPath(path);
                                isError = true;
                            }
                        }
                        else
                        {
                            Error.ConflictArguments(arguments[i].Value);
                            isError = true;
                        }
                    }
                    else
                    {
                        isError = true;
                    }
                }

                if (isError || modPath == null)
                {
                    Error.ArgumentsError("Trans-Custom");
                }
                else
                {
                    string earlyTargetLanguage = Config.TargetLanguage;
                    if (lang != null) Config.TargetLanguage = lang;
                    Option.ModInfo modInfo = new Option.ModInfo(modPath, output);
                    if (modInfo.IsValidMod)
                    {
                        Mod mod;
                        if (string.Compare(Path.GetFileName(modPath), "core", true) == 0)
                        {
                            mod = new Mod(modInfo);
                        }
                        else
                        {
                            mod = new Mod(modInfo, Mod.Core);
                        }

                        Console.WriteLine("Mod: {0}", modInfo.ModPath);
                        mod.Generate();
                        mod.Export();
                        Console.WriteLine("Output to: {0}", modInfo.TargetLanguage);
                    }
                    else
                    {
                        Error.InvalidMod(modPath);
                    }
                    Config.TargetLanguage = earlyTargetLanguage;
                }
            }
            else
            {
                Error.ArgumentsError("Trans-Custom");
            }
        }
    }
}
