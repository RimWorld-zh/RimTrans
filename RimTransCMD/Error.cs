using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimTrans.Cmd
{
    static class Error
    {
        public static void UnAvailableCommand(string command)
        {
            Console.WriteLine("ERROR: invalid command \"{0}\".", command);
        }

        public static void ArgumentsError(string command)
        {
            Console.WriteLine("Command <{0}> ERROR: invalid arguments, please check the format.", command);
        }

        public static void InvalidArguments(string argument)
        {
            Console.WriteLine("ERROR: invalid argument \"{0}\", please check spelling or view help.", argument);
        }

        public static void ConflictArguments(string argument)
        {
            Console.WriteLine("ERROR: conflict argument \"{0}\", arguments cannot be repeated.", argument);
        }

        public static void DirectoryNotFound(string path)
        {
            Console.WriteLine("ERROR: directory not found \"{0}\".", path);
        }

        public static void InvalidTargetLanguage(string targetLanguage)
        {
            Console.WriteLine("ERROR: invalid target language \"{0}\", only English letters are allowed.", targetLanguage);
        }
    }
}
