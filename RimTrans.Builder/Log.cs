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
            random = new Random();
        }

        static Random random;

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
        public static void FaceGodd()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(faceTextGood[random.Next(9)]);
            Console.ResetColor();
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
        public static void FaceBad()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(faceTextBad[random.Next(9)]);
            Console.ResetColor();
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
            //Console.Write(faceTextGood[random.Next(9)]);
            Console.Write("INFO     ");
            Console.ResetColor();
        }

        public static void Warning()
        {
            //nsole.Write("XXXXXXX  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.Write(faceTextBad[random.Next(9)]);
            Console.Write("WARNING  ");
            Console.ResetColor();
        }

        public static void Error()
        {
            //nsole.Write("XXXXXXX  ");
            Console.ForegroundColor = ConsoleColor.Red;
            //Console.Write(faceTextBad[random.Next(9)]);
            Console.Write("ERROR    ");
            Console.ResetColor();
        }

        #endregion

        #region Write

        public static void Write(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        public static void Write(string text)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(text);
            Console.ResetColor();
        }

        public static void Write(ConsoleColor color, string format, params object[] arg)
        {
            Console.ForegroundColor = color;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        public static void Write(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(format, arg);
            Console.ResetColor();
        }

        #endregion

        #region WriteLine

        public static void WriteLine(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void WriteLine(string text)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void WriteLine(ConsoleColor color, string format, params object[] arg)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(format, arg);
            Console.ResetColor();
        }

        public static void WriteLine(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(format, arg);
            Console.ResetColor();
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }

        #endregion


    }
}
