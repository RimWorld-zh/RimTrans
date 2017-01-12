using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RimTransLite.Options
{
    public class LiteConfigs
    {
        static LiteConfigs()
        {

        }

        public static string PathRimWorld
        {
            get { return @"D:\Game\Steam\steamapps\common\RimWorld"; }
        }

        public static string PathInternalMods
        {
            get { return Path.Combine(PathRimWorld, "Mods"); }
        }

        public static string PathCore
        {
            get { return Path.Combine(PathInternalMods, "Core"); }
        }

        public static string PathWorkshopMods
        {
            get { return @"D:\Game\Steam\steamapps\workshop\content\294100"; }
        }
    }
}
