using System;
using System.Linq;

namespace RimTrans.Option
{
    public static class PathsX
    {
        private static char[] invalidChars = new char[]
        { 
            '\n', '\r', '\t', '\\', '/', ':', '*', '?', '\"', '<', '>', '|'
        };

        /// <summary>
        /// Remove invalid characters in this file/folder name.
        /// </summary>
        public static string TrimPath(this string name)
        {
            foreach (char invalidChar in invalidChars)
            {
                name = name.Replace(invalidChar, ' ');
            }
            name = name.Trim();
            return name;
        }
    }
}
