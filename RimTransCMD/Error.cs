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

        public static void InvalidArguments()
        {
            Console.WriteLine("Command <Info> ERROR: invalid arguments.");
        }

        public static void NeedArguments()
        {
            Console.WriteLine("Command <Info> ERROR: need arguments.");
        }
    }
}
