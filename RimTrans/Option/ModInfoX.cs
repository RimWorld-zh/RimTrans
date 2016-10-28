using System;
using System.Linq;
using System.IO;

namespace RimTrans.Option
{
    public static class ModInfoX
    {
        /// <summary>
        /// Remove invalid characters in this file/folder name.
        /// </summary>
        public static string TrimName(this string name)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar, ' ');
            }
            name = name.Trim();
            return name;
        }
    }
}
