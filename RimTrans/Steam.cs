using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Steam.Local;

namespace RimTrans
{
    public static class Steam
    {
        /// <summary>
        /// Get the directory of RimWorld
        /// </summary>
        public static string GetDirRimWorld()
        {
            string dir = string.Empty;
            try
            {
                LocalApp rw = new LocalApp(294100); // 294100: the AppID of RimWorld.
                dir = rw.InstallDir;
                if (dir.IndexOf(@":\") == 1)
                {
                    dir = dir[0].ToString().ToUpper() + dir.Substring(1);
                }
            }
            catch (Exception)
            {
                dir = @"C:\Program Files (x86)\Steam\steamapps\common\RimWorld";
            }
            return dir;
        }

        /// <summary>
        /// Get the directory of Workshop items of RimWorld
        /// </summary>
        public static string GetDirModsWorkshop()
        {
            string dir = string.Empty;
            try
            {
                LocalApp rw = new LocalApp(294100); // 294100: the AppID of RimWorld.
                dir = rw.Workshop.Path;
                dir = Path.GetDirectoryName(dir);
                dir = Path.Combine(dir, @"content\294100");
                if (dir.IndexOf(@":\") == 1)
                {
                    dir = dir[0].ToString().ToUpper() + dir.Substring(1);
                }
            }
            catch (Exception)
            {
                dir = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\294100";
            }
            return dir;
        }
    }
}
