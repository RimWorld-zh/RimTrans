using System;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace RimTrans.Option
{
    public class ModInfo
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
        
        /// <summary>
        /// Initial ModInfo as Core.
        /// </summary>
        public ModInfo()
            :this("Core", Where.Direct)
        {
        }

        /// <summary>
        /// Initial ModInfo by custom path
        /// </summary>
        public ModInfo(string pathCustom)
        {
            this.PathCustom = pathCustom;
            this.Name = Path.GetFileName(pathCustom);
            this.Where = Where.Custom;
            try
            {
                this.docAbout = XDocument.Load(this.AboutFile);
            }
            catch (Exception)
            {
                this.docAbout = null;
                //TODO: Log
            }
        }

        /// <summary>
        /// Initial ModInfo in where direct or worshop. If custom, throw exception.
        /// </summary>
        public ModInfo(string modName, Where where)
        {
            if (where == Where.Custom) throw new Exception("Argument exception: the where can not be RimTrans.Option.Where.Custom");

            this.Name = modName;
            this.Where = where;
            try
            {
                this.docAbout = XDocument.Load(this.AboutFile);
            }
            catch (Exception)
            {
                this.docAbout = null;
                //TODO: Log
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
        /// About.xml
        /// </summary>
        private XDocument docAbout;

        /// <summary>
        /// The folder name, it will be specially generated into direct.
        /// </summary>
        public string FolderName
        {
            get
            {
                string result = string.Empty;
                if (this.Where == Where.Direct || this.Where == Where.Custom)
                {
                    result = this.Name;
                }
                else
                {
                    try
                    {
                        result += this.Auther + " ";
                        result += this.ViewName + " ";
                        result += this.Name;
                        result = result.TrimPath();
                    }
                    catch (Exception)
                    {
                        result = this.Name;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Which be written in the tag &lt;name&gt; in the About.xml.
        /// </summary>
        public string ViewName
        {
            get
            {
                string result = string.Empty;
                try
                {
                    result = this.docAbout.Root.Element("name").Value.Trim();
                }
                catch (Exception)
                {
                    result = "Untitled";
                }
                return result;
            }
        }


        /// <summary>
        /// Which be written in the tag &lt;author&gt; in the About.xml.
        /// </summary>
        public string Auther
        {
            get
            {
                string result = string.Empty;
                try
                {
                    result = this.docAbout.Root.Element("author").Value.Trim();
                }
                catch (Exception)
                {
                    result = "Anonymous";
                }
                return result;
            }
        }

        private string PathCustom { get; set; }

        /// <summary>
        /// The directory of this Mod.
        /// </summary>
        public string ModPath
        {
            get
            {
                string path;
                if (this.Where == Where.Direct)
                    path = Path.Combine(Config.DirModsDirect, this.Name);
                else if (this.Where == Where.Workshop)
                    path = Path.Combine(Config.DirModsWorkshop, this.Name);
                else
                    path = this.PathCustom;
                return path;
            }
        }

        /// <summary>
        /// Folder About
        /// </summary>
        public string About { get { return Path.Combine(this.ModPath, FF.About); } }

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
        public string Defs { get { return Path.Combine(this.ModPath, FF.Defs); } }

        /// <summary>
        /// Folder Languages (direct)
        /// </summary>
        public string Languages
        {
            get
            {
                string path;
                if (this.Where == Where.Direct)
                    path = Path.Combine(this.ModPath, FF.Languages);
                else if (this.Where == Where.Workshop)
                    path = Path.Combine(Config.DirModsDirect, this.FolderName, FF.Languages);
                else
                    path = Path.Combine(this.ModPath, FF.Languages);
                return path;
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

        private string LanguagesOriginal { get { return Path.Combine(this.ModPath, FF.Languages); } }

        /// <summary>
        /// Folder Original Languages, usually is English, like "Rimworld\Mods\[the Mod]\Languages\English"
        /// </summary>
        public string OriginalLanguage
        {
            get
            {
                string result = Path.Combine(this.LanguagesOriginal, FF.English);
                if (Directory.Exists(result) == false && Directory.Exists(this.LanguagesOriginal))
                {
                    foreach (string path in Directory.GetDirectories(this.LanguagesOriginal))
                    {
                        if (path != this.TargetLanguage)
                        {
                            result = path;
                            break;
                        }
                    }
                }
                return result;
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

        public override string ToString()
        {
            string result = string.Empty;
            if (Where == Where.Direct)
            {
                result += "Mod: ";
                result += this.Name;
                result += ", Author: ";
                result += this.Auther;
            }
            else
            {
                result += "ID: ";
                result += this.Name;
                result += ", Mod: ";
                result += this.ViewName;
                result += ", Author: ";
                result += this.Auther;
            }
            
            return result;
        }

        /// <summary>
        /// Is this a valid mod.
        /// </summary>
        public bool IsValid
        {
            get
            {
                bool result = false;
                if (Directory.Exists(this.ModPath))
                {
                    foreach (string subDir in Directory.GetDirectories(this.ModPath))
                    {
                        if (subDir == "Defs" || subDir == "Assemblies")
                        {
                            result = true;
                            break;
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// The name of subdirectories in DefInjected are Singular and Plural.
        /// </summary>
        public bool IsFolderFomatWell
        {
            get
            {
                bool result = true;
                if (Directory.Exists(this.DefsInjected))
                {
                    foreach (string subDir in Directory.GetDirectories(this.DefsInjected))
                    {
                        string subDirName = Path.GetFileName(subDir);
                        if (subDirName.LastIndexOf("Def") == subDirName.LastIndexOf("Defs"))
                        {
                            result = false;
                            break;
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Change subdirectories' name plural to singular in the folder DefInjected.
        /// </summary>
        public void FomatFolderName()
        {
            if (Directory.Exists(this.DefsInjected))
            {
                foreach (string sourceDir in Directory.GetDirectories(this.DefsInjected))
                {
                    string sourceDirName = Path.GetFileName(sourceDir);
                    if (sourceDirName.LastIndexOf("Def") == sourceDirName.LastIndexOf("Defs"))
                    {
                        string destDirName = sourceDirName.Substring(0, sourceDirName.Length - 1);
                        string destDir = Path.Combine(Path.GetDirectoryName(sourceDir), destDirName);
                        if (Directory.Exists(destDir))
                        {
                            foreach (string sourceFile in Directory.GetFiles(sourceDir))
                            {
                                string destFile = Path.Combine(destDir, Path.GetFileName(sourceFile));
                                if (File.Exists(destFile))
                                {
                                    int ordinal = 0;
                                    while (File.Exists(destFile))
                                    {
                                        ordinal++;
                                        string destFileName = Path.GetFileNameWithoutExtension(sourceFile);
                                        destFileName += string.Format(" ({0})", ordinal.ToString());
                                        if (Path.HasExtension(sourceFile))
                                        {
                                            destFileName += Path.GetExtension(sourceFile);
                                        }
                                        destFile = Path.Combine(destDir, destFileName);
                                    }
                                    File.Copy(sourceFile, destFile, true);
                                    File.Delete(sourceFile);
                                }
                                else
                                {
                                    File.Move(sourceFile, destFile);
                                }
                            }
                            try
                            {
                                Directory.Delete(sourceDir);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else
                        {
                            Directory.Move(sourceDir, destDir);
                        }
                    }
                }
            }
        }

        public void _Debug()
        {
            System.Reflection.PropertyInfo[] pis = typeof(ModInfo).GetProperties();
            foreach (var pi in pis)
            {
                Console.WriteLine(pi.Name);
                Console.WriteLine(pi.GetValue(this));
            }
        }
    }
}
