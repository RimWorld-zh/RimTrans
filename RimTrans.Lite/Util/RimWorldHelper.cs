using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimTrans.Lite.Util
{
    public static class RimWorldHelper
    {
        public static string GetRimWorldInstallDir()
        {
            return @"D:\Game\Steam\steamapps\common\RimWorld";
        }

        public static string GetInternalModsDir()
        {
            string rwDir = UserSettings.All.RimWorldInstallDir;
            if (string.IsNullOrWhiteSpace(rwDir))
            {
                rwDir = GetRimWorldInstallDir();
                if (string.IsNullOrWhiteSpace(rwDir))
                {
                    return null;
                }
            }
            UserSettings.All.RimWorldInstallDir = rwDir;
            return Path.Combine(rwDir, "Mods");
        }
    }
}
