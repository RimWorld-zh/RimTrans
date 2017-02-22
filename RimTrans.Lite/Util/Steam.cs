using System;
using System.IO;
using Steam.Local;

namespace RimTrans.Lite.Util
{
    public static class Steam
    {
        public static string GetRimWorldDirectory()
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

        public static string GetWorkshopDirectory()
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
