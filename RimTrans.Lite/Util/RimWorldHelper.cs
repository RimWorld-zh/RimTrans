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

        public static string GetInternalModsDir()
        {
            string rwDir = GetRimWorldInstallDir();
            if (string.IsNullOrWhiteSpace(rwDir))
            {
                return string.Empty;
            }
            else
            {
                return Path.Combine(rwDir, "Mods");
            }
        }

        public static string GetRimWorldInstallDir()
        {
            string rwDir = UserSettings.All.RimWorldInstallDir;
            if (string.IsNullOrWhiteSpace(rwDir))
            {
                rwDir = @"D:\Game\Steam\steamapps\common\RimWorld";
                if (string.IsNullOrWhiteSpace(rwDir))
                {
                    rwDir = string.Empty;
                }
                UserSettings.All.RimWorldInstallDir = rwDir;
            }
            return rwDir;
        }

        public static string GetWorkshopModsDir()
        {
            string wsDir = UserSettings.All.Workshop294100;
            if (string.IsNullOrWhiteSpace(wsDir))
            {
                wsDir = @"D:\Game\Steam\steamapps\workshop\content\294100";
                if (string.IsNullOrWhiteSpace(wsDir))
                {
                    wsDir = string.Empty;
                }
                UserSettings.All.Workshop294100 = wsDir;
            }
            return wsDir;
        }
    }
}
