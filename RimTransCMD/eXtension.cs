using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RimTrans.Cmd
{
    static class eXtension
    {
        public static bool IsValidLanguage(this string targetLanguage)
        {
            bool result = false;
            foreach (Match match in Regex.Matches(targetLanguage, "[A-Za-z]{1,}"))
            {
                if (match.Value == targetLanguage)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        
        public static bool IsValidPath(this string path)
        {
            bool result = false;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                result = true;
            }
            catch (Exception)
            { 
            }
            return result;
        }
    }
}
