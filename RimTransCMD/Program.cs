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
            Config.DirRimWorld = SteamX.GetDirRimWorld();
            Config.DirModsWorkshop = SteamX.GetDirModsWorkshop();
            CommandStart(); 

            while (true)
            {
                Console.Write("RimTrans> ");
                string cmdLine = Console.ReadLine();
                if (cmdLine != string.Empty)
                {
                    string command = string.Empty;
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

                    if (command != string.Empty)
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
                        else if (string.Compare(command, "trans-direct", true) == 0)
                        {

                        }
                        else if (string.Compare(command, "trans-workshop", true) == 0)
                        {

                        }
                        else if (string.Compare(command, "trans-custom", true) == 0)
                        {

                        }
                        else if (string.Compare(command, "trans-core", true) == 0)
                        {

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
            Console.WriteLine("RimTrans CMD [Version: {0}]", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("MIT License, Copyright (c) 2016 duduluu");
            Console.WriteLine();
            DisplayInfo();
            Console.WriteLine();

        }

        static void DisplayInfo()
        {
            Console.WriteLine("Installation directory of RimWorld: {0}", Config.DirRimWorld);
            Console.WriteLine("Workshop directory of RimWorld: {0}", Config.DirModsWorkshop);
            Console.WriteLine("Current Target Language: {0}", Config.TargetLanguage);
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
                    foreach (var modInfo in Config.GetModInfos(Option.Where.Direct | Option.Where.Workshop))
                    {
                        Console.WriteLine(modInfo);
                    }
                }
                else if (string.Compare(argument, "/direct") == 0)
                {
                    foreach (var modInfo in Config.GetModInfos(Option.Where.Direct))
                    {
                        Console.WriteLine(modInfo);
                    }
                }
                else if (string.Compare(argument, "/workshop") == 0)
                {
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
                string gameDir = string.Empty;
                string workshopDir = string.Empty;
                string targetLanguage = string.Empty;

                bool isError = false;
                for (int i = 0; i < arguments.Count() && i < headers.Count(); i++)
                {
                    // Game-Dir
                    if (string.Compare(headers[i].Value, "/game-dir", true) == 0)
                    {
                        string path = arguments[i].Value.Replace(headers[i].Value, string.Empty).Substring(1).Replace("\"", string.Empty);
                        if (gameDir == string.Empty)
                        {
                            if (Directory.Exists(path))
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
                        if (workshopDir == string.Empty)
                        {
                            if (Directory.Exists(path))
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
                        if (targetLanguage == string.Empty)
                        {
                            bool isValid = false;
                            foreach (Match match in Regex.Matches(targetLanguage, "[A-Za-z]{1,}"))
                            {
                                if (match.Value == targetLanguage)
                                {
                                    isValid = true;
                                    break;
                                }
                            }
                            if (isValid)
                            {
                                targetLanguage = language;
                            }
                            else
                            {
                                Error.InvalidArguments(targetLanguage);
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
                    if (gameDir != string.Empty)
                    {
                        Config.DirRimWorld = gameDir;
                        Console.WriteLine("Installation directory of RimWorld: {0}", Config.DirRimWorld);
                    }
                    if (workshopDir != string.Empty)
                    {
                        Config.DirModsWorkshop = workshopDir;
                        Console.WriteLine("Workshop directory of RimWorld: {0}", Config.DirModsWorkshop);
                    }
                    if (targetLanguage != string.Empty)
                    {
                        Config.TargetLanguage = targetLanguage;
                        Console.WriteLine("Current Target Language: {0}", Config.TargetLanguage);
                    }
                }
            }
            else
            {
                Error.ArgumentsError("Set");
            }
        }

        /// <summary>
        /// Commadn Trans-Direct
        /// </summary>
        static void CommandTransDirect(List<Match> arguments, List<Match> headers)
        {
            if (arguments.Count() > 0 && arguments.Count() == headers.Count())
            {
                string modName = string.Empty;
                string lang = string.Empty;
                //string output = string.Empty;

            }
            else
            {
                Error.ArgumentsError("Trans-Direct");
            }
        }

        /// <summary>
        /// Commadn Trans-Workshop
        /// </summary>
        static void CommandTransWorkshop(List<Match> arguments, List<Match> headers)
        {

        }

        /// <summary>
        /// Commadn Trans-Custom
        /// </summary>
        static void CommandTransCustom(List<Match> arguments, List<Match> headers)
        {

        }
    }
}
