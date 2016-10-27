using System;
using System.Collections.Generic;
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
                            CommandSet(arguments);
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
                if (string.Compare(argument, "all") == 0)
                {
                    foreach (var modInfo in Config.GetModInfos(Option.Where.Direct | Option.Where.Workshop))
                    {
                        Console.WriteLine(modInfo);
                    }
                }
                else if (string.Compare(argument, "direct") == 0)
                {
                    foreach (var modInfo in Config.GetModInfos(Option.Where.Direct))
                    {
                        Console.WriteLine(modInfo);
                    }
                }
                else if (string.Compare(argument, "workshop") == 0)
                {
                    foreach (var modInfo in Config.GetModInfos(Option.Where.Workshop))
                    {
                        Console.WriteLine(modInfo);
                    }
                }
                else
                {
                    Error.InvalidArguments();
                }
            }
            else
            {
                Error.InvalidArguments();
            }
        }

        /// <summary>
        /// Commadn Set
        /// </summary>
        static void CommandSet(List<Match> arguments)
        {
            if (arguments.Count() == 0)
            {
                Error.NeedArguments();
            }
            else
            {
                string dirRimWorld = string.Empty;
                string dirModsWorkshop = string.Empty;
                string targetLanguage = string.Empty;

                bool isError = false;
                foreach (var arg in arguments)
                {
                    string argName = string.Empty;
                    string argValue = string.Empty;
                    foreach (Match head in Regex.Matches(arg.Value, patternHeader))
                    {
                        argName = head.Value;
                        argValue = argValue.Replace(argName, string.Empty);
                        argValue = argValue.Replace(":", string.Empty);
                        break;
                    }
                    if (string.Compare(argName, "game-dir", true) == 0)
                    {
                        dirRimWorld = argValue;
                    }
                    else if (string.Compare(argName, "workshop-dir", true) == 0)
                    {

                    }
                    else if (string.Compare(argName, "target-language", true) == 0)
                    {

                    }
                    else
                    {
                        isError = false;
                    }
                }
            }
        }

        /// <summary>
        /// Commadn Trans-Direct
        /// </summary>
        static void CommandTransDirect(List<Match> arguments)
        {

        }

        /// <summary>
        /// Commadn Trans-Workshop
        /// </summary>
        static void CommandTransWorkshop(List<Match> arguments)
        {

        }

        /// <summary>
        /// Commadn Trans-Custom
        /// </summary>
        static void CommandTransCustom(List<Match> arguments)
        {

        }
    }
}
