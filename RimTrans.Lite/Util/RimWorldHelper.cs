using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using SteamKit2;

namespace RimTrans.Lite.Util
{
    public static class RimWorldHelper
    {
        public static string RimWorldInstallDir { get; private set; } = string.Empty;

        public static string WorkshopModsDir { get; private set; } = string.Empty;

        static RimWorldHelper()
        {
            RegistryKey steamRegistryKey =
                Environment.Is64BitOperatingSystem ?
                steamRegistryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam", false) :
                steamRegistryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam", false);
            if (steamRegistryKey == null)
                return;

            var value = steamRegistryKey.GetValue("InstallPath");
            if (value == null)
                return;

            string steamInstallPath = value.ToString();
            if (string.IsNullOrWhiteSpace(steamInstallPath))
                return;

            string defaultLibrary = Path.Combine(steamInstallPath, "steamapps");
            List<string> libraryFolders = new List<string>() { defaultLibrary };

            string libraryfoldersFile = Path.Combine(defaultLibrary, "libraryfolders.vdf");
            KeyValue kvLibraryFolders = new KeyValue();
            if (kvLibraryFolders.ReadFileAsText(libraryfoldersFile))
            {
                foreach (KeyValue child in kvLibraryFolders.Children)
                {
                    int index;
                    if (int.TryParse(child.Name, out index))
                    {
                        libraryFolders.Add(Path.Combine(child.Value, "steamapps"));
                    }
                }
            }

            foreach (string folder in libraryFolders)
            {
                if (File.Exists(Path.Combine(folder, "appmanifest_294100.acf")))
                {
                    RimWorldInstallDir = Path.Combine(folder, "common", "RimWorld");
                    WorkshopModsDir = Path.Combine(folder, "workshop", "content", "294100");
                    return;
                }
            }
        }
    }
}
