using System;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace RimTrans.Option
{
    public class Paths
    {
        /// <summary>
        /// folders and files
        /// </summary>
        private static class FF
        {
            public static readonly string Mod = "Mod";
            public static readonly string About = "About";
            public static readonly string AboutXml = "About.xml";
            public static readonly string AboutPreview = "Preview.png";
            public static readonly string Assemblies = "Assemblies";
            public static readonly string Defs = "Defs";
            public static readonly string Languages = "Languages";
            public static readonly string English = "English";
            public static readonly string DefInjected = "DefInjected";
            public static readonly string Keyed = "Keyed";
            public static readonly string Strings = "Strings";
            public static readonly string Textures = "Textures";
        }

        public Paths()
            :this("Core", Where.Direct)
        {
        }

        public Paths(string modName, Where where)
        {
            this.Name = modName;
            this.Where = where;
            if (this.Where == Where.Direct)
                this.FolderName = this.Name;
            else
            {
                try
                {
                    XDocument aboutXML = XDocument.Load(this.AboutFile);
                    this.FolderName = string.Empty;
                    this.FolderName += aboutXML.Root.Element("author").Value.Trim() + " ";
                    this.FolderName += aboutXML.Root.Element("name").Value.Trim() + " ";
                    this.FolderName += this.Name;
                    this.FolderName = this.FolderName.TrimPath();
                }
                catch (Exception)
                {
                    this.FolderName = this.Name;
                }
            }
        }

        /// <summary>
        /// Name of this Mod, just the folder name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Where is the Mod, the direct Mods folder or the Workshop folder.
        /// </summary>
        public Where Where { get; private set; }

        /// <summary>
        /// The folder name, it will be specially generated into direct.
        /// </summary>
        public string FolderName { get; private set; }

        public string Dir
        {
            get
            {
                if (this.Where == Where.Direct)
                    return Path.Combine(Config.DirModsDirect, this.Name);
                else
                    return Path.Combine(Config.DirModsWorkshop, this.Name);
            }
        }

        /// <summary>
        /// Folder About
        /// </summary>
        public string About { get { return Path.Combine(this.Dir, FF.About); } }

        /// <summary>
        /// File About.xml
        /// </summary>
        public string AboutFile { get { return Path.Combine(this.About, FF.AboutXml); } }

        /// <summary>
        /// File Preview.png
        /// </summary>
        public string AboutPreview { get { return Path.Combine(this.About, FF.AboutPreview); } }

        /// <summary>
        /// Folder Defs
        /// </summary>
        public string Defs { get { return Path.Combine(this.Dir, FF.Defs); } }

        /// <summary>
        /// Folder Languages (direct)
        /// </summary>
        public string Languages
        {
            get
            {
                return Path.Combine(Config.DirModsDirect, this.FolderName, FF.Languages);
            }
        }

        /// <summary>
        /// Folder TargetLanguage, like "Rimworld\Mods\[the Mod]\Languages\ChineseSimplified"
        /// </summary>
        public string TargetLanguage
        {
            get
            {
                return Path.Combine(this.Languages, Config.TargetLanguage);
            }
        }

        /// <summary>
        /// Folder DefInjected
        /// </summary>
        public string DefsInjected { get { return Path.Combine(this.TargetLanguage, FF.DefInjected); } }

        /// <summary>
        /// Folder Keyed
        /// </summary>
        public string Keyed { get { return Path.Combine(this.TargetLanguage, FF.Keyed); } }

        /// <summary>
        /// Folder Strings
        /// </summary>
        public string Strings { get { return Path.Combine(this.TargetLanguage, FF.Strings); } }

        /// <summary>
        /// Folder Original Languages, usually is English, like "Rimworld\Mods\[the Mod]\Languages\English"
        /// </summary>
        public string OriginalLanguage
        {
            get
            {
                string result = Path.Combine(this.Dir, FF.Languages, FF.English);
                if (Directory.Exists(result))
                    return result;
                else
                {
                    try
                    {
                        foreach (string path in Directory.GetDirectories(Path.Combine(this.Dir, FF.Languages)))
                        {
                            if (path != this.TargetLanguage)
                            {
                                result = path;
                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// Folder Keyed (original), usually is English.
        /// </summary>
        public string KeyedOriginal { get { return Path.Combine(OriginalLanguage, FF.Keyed); } }

        /// <summary>
        /// Folder Strings (original), usually is English. 
        /// </summary>
        public string StringsOriginal { get { return Path.Combine(OriginalLanguage, FF.Strings); } }

        public void _Debug()
        {
            System.Reflection.PropertyInfo[] pis = typeof(Paths).GetProperties();
            foreach (var pi in pis)
            {
                Console.WriteLine(pi.Name);
                Console.WriteLine(pi.GetValue(this));
                Console.WriteLine();
            }
        }
    }
}
