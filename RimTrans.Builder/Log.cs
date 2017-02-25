using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimTrans.Builder
{
    public static class Log
    {
        static Log()
        {
            //Console.SetWindowSize(116, 35);
            //Console.SetBufferSize(116, 3000);
        }

        #region Head

        public static void Indent()
        {
            //nsole.Write("XXXXXXX  ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("         ");
            Console.ResetColor();
        }

        public static void Info()
        {
            //nsole.Write("XXXXXXX  ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("INFO     ");
            Console.ResetColor();
        }

        public static void Warning()
        {
            //nsole.Write("XXXXXXX  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("WARNING  ");
            Console.ResetColor();
        }

        public static void Error()
        {
            //nsole.Write("XXXXXXX  ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR    ");
            Console.ResetColor();
        }

        #endregion

        #region Write

        public static void Write(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void WriteLine(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(format, arg);
            Console.ResetColor();
        }

        public static void Line()
        {
            Console.WriteLine();
        }

        #endregion


        #region Color

        public static void Black(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void DarkBlue(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void DarkGreen(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void DarkCyan(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void DarkRed(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void DarkMagenta(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void DarkYellow(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void Gray(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void DarkGray(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void Blue(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void Green(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void Cyan(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void Red(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void Magenta(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void Yellow(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void White(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        #endregion


    }
}
