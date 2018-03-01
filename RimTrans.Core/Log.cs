using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RimTrans.Core {
    public static class Log {
        public static readonly List<Action<string, Exception>> InfoHandler = new List<Action<string, Exception>>();

        public static async void Info(string message, Exception exception = null) {
            Console.WriteLine(message);
        }


        public static readonly List<Action<string, Exception>> WarningHandler = new List<Action<string, Exception>>();
        
        public static async void Warning(string message, Exception exception = null) {
            Console.WriteLine(message);
        }


        public static readonly List<Action<string, Exception>> ErrorHandler = new List<Action<string, Exception>>();
        
        public static async void Error(string message, Exception exception = null) {
            Console.WriteLine(message);
        }
    }
}
