using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RimTrans
{
    static class Config
    {
        /// <summary>
        /// Config file location.
        /// </summary>
        public static string FilePath { get; set; }

        /// <summary>
        /// Storge config.
        /// </summary>
        public static XDocument doc = XDocument.Parse(Resources.DefaultConfig);

        /// <summary>
        /// Defualt config.
        /// </summary>
        public static readonly XDocument defaultDoc = XDocument.Parse(Resources.DefaultConfig);


        #region 方法

        /// <summary>
        /// Load config file.
        /// </summary>
        public static void Load(string configFilePath)
        {
            Config.FilePath = configFilePath;
            try
            {
                Config.Parse(XDocument.Load(Config.FilePath));
            }
            catch (Exception)
            {
                Config.Parse(XDocument.Parse(Resources.DefaultConfig));
                Config.Save();
            }
            Config.doc.Changed += ChangedSave;
        }

        /// <summary>
        /// Parse config from xml document.
        /// </summary>
        private static void Parse(XDocument other)
        {
            foreach (XElement element in Config.doc.Root.Elements())
            {
                if (other.Root.Element(element.Name).Value == null ||
                    other.Root.Element(element.Name).Value == string.Empty)
                    throw new Exception("Config File Error!");
                element.Value = other.Root.Element(element.Name).Value;
            }
        }

        /// Reset config to default
        /// </summary>
        public static void Reset()
        {
            Config.doc.Changed -= ChangedSave;
            Config.Parse(XDocument.Parse(Resources.DefaultConfig));
            Config.Save();
            Config.doc.Changed += ChangedSave;
        }

        /// <summary>
        /// Save config file.
        /// </summary>
        public static void Save()
        {
            try
            {
                Config.doc.Save(Config.FilePath);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Save after change
        /// </summary>
        private static void ChangedSave(Object sender, XObjectChangeEventArgs e)
        {
            Config.Save();
        }

        #endregion

        #region Directory 目录

        public static string TargetLanguage
        {
            get { return Config.doc.Root.Element("TargetLanguage").Value; }
            set { Config.doc.Root.Element("TargetLanguage").Value = value; }
        }

        public static string DirSteamApps
        {
            get { return Config.doc.Root.Element("DirSteamApps").Value; }
            set { Config.doc.Root.Element("DirSteamApps").Value = value; }
        }

        public static string DirRimWorld
        {
            get { return Config.doc.Root.Element("DirRimWorld").Value; }
            set { Config.doc.Root.Element("DirRimWorld").Value = value; }
        }

        public static string DirModsDirect
        {
            get { return Config.DirRimWorld + @"\Mods"; }
        }

        public static string DirModsWorkshop
        {
            get { return Config.DirSteamApps + @"\workshop\content\294100"; }
        }

        public static string SelectedMod
        {
            get { return Config.doc.Root.Element("SelectedMod").Value; }
            set { Config.doc.Root.Element("SelectedMod").Value = value; }
        }

        public static RimTrans.Option.Where SelectedWhere
        {
            get
            {
                XElement key = Config.doc.Root.Element("SelectedWhere");
                if (key.Value == "Direct")
                    return RimTrans.Option.Where.Direct;
                else if (key.Value == "Workshop")
                    return RimTrans.Option.Where.Workshop;
                else
                {
                    key.Value = "Direct";
                    return RimTrans.Option.Where.Direct;
                }
            }
            set
            {
                if (value == RimTrans.Option.Where.Direct)
                    Config.doc.Root.Element("SelectedWhere").Value = "Direct";
                else if (value == RimTrans.Option.Where.Workshop)
                    Config.doc.Root.Element("SelectedWhere").Value = "Workshop";
            }
        }

        #endregion

        #region Typesetting 排版

        public static int IndentSize
        {
            get
            {
                XElement key = Config.doc.Root.Element("IndentSize");
                int result;
                if (int.TryParse(key.Value, out result) &&
                    0 <= result && result <= 10)
                {
                    return result;
                }
                else
                {
                    key.Value = Config.defaultDoc.Root.Element("IndentSize").Value;
                    return 0;
                }
            }
            set
            {
                XElement key = Config.doc.Root.Element("IndentSize");
                if (0 <= value && value <= 10)
                    key.Value = value.ToString();
                else
                    key.Value = Config.defaultDoc.Root.Element(key.Name).Value;
            }
        }

        private static readonly string[] indent = new string[] 
        { 
            string.Empty,
            new string(' ', 1), new string(' ', 2), new string(' ', 3),
            new string(' ', 4), new string(' ', 5), new string(' ', 6),
            new string(' ', 7), new string(' ', 8), new string(' ', 9),
            new string(' ', 10)
        };

        public static string IndentSpaces
        {
            get { return indent[Config.IndentSize]; }
        }

        public static bool IsCommentExtract
        {
            get
            {
                XElement key = Config.doc.Root.Element("IsCommentExtract");
                bool result;
                if (bool.TryParse(key.Value, out result))
                    return result;
                else
                {
                    key.Value = Config.defaultDoc.Root.Element(key.Name).Value;
                    result = bool.Parse(key.Value);
                    return result;
                }
            }
            set { Config.doc.Root.Element("IsCommentExtract").Value = value.ToString(); }
        }

        public static bool IsCommentTypesetting
        {
            get
            {
                XElement key = Config.doc.Root.Element("IsCommentTypesetting");
                bool result;
                if (bool.TryParse(key.Value, out result))
                    return result;
                else
                {
                    key.Value = Config.defaultDoc.Root.Element(key.Name).Value;
                    result = bool.Parse(key.Value);
                    return result;
                }
            }
            set { Config.doc.Root.Element("IsCommentTypesetting").Value = value.ToString(); }
        }
        
        #endregion

        #region Generating 生成

        public static bool IsPatchStuffAdjective
        {
            get
            {
                XElement key = Config.doc.Root.Element("IsPatchStuffAdjective");
                bool result;
                if (bool.TryParse(key.Value, out result))
                    return result;
                else
                {
                    key.Value = Config.defaultDoc.Root.Element(key.Name).Value;
                    result = bool.Parse(key.Value);
                    return result;
                }
            }
            set { Config.doc.Root.Element("IsPatchStuffAdjective").Value = value.ToString(); }
        }

        public static bool IsPatchPawnGenders
        {
            get
            {
                XElement key = Config.doc.Root.Element("IsPatchPawnGenders");
                bool result;
                if (bool.TryParse(key.Value, out result))
                    return result;
                else
                {
                    key.Value = Config.defaultDoc.Root.Element(key.Name).Value;
                    result = bool.Parse(key.Value);
                    return result;
                }
            }
            set { Config.doc.Root.Element("IsPatchPawnGenders").Value = value.ToString(); }
        }

        public static bool IsGenBuildingExtra
        {
            get
            {
                XElement key = Config.doc.Root.Element("IsGenBuildingExtra");
                bool result;
                if (bool.TryParse(key.Value, out result))
                    return result;
                else
                {
                    key.Value = Config.defaultDoc.Root.Element(key.Name).Value;
                    result = bool.Parse(key.Value);
                    return result;
                }
            }
            set { Config.doc.Root.Element("IsGenBuildingExtra").Value = value.ToString(); }
        }

        #endregion

        #region Files&Fileds Scheme 文件与字段方案

        public static bool IsFieldsExistingAdopt
        {
            get
            {
                XElement key = Config.doc.Root.Element("IsFieldsExistingAdopt");
                bool result;
                if (bool.TryParse(key.Value, out result))
                    return result;
                else
                {
                    key.Value = Config.defaultDoc.Root.Element(key.Name).Value;
                    result = bool.Parse(key.Value);
                    return result;
                }
            }
            set { Config.doc.Root.Element("IsFieldsExistingAdopt").Value = value.ToString(); }
        }
        public static bool IsFieldsInvalidHold
        {
            get
            {
                XElement key = Config.doc.Root.Element("IsFieldsInvalidHold");
                bool result;
                if (bool.TryParse(key.Value, out result))
                    return result;
                else
                {
                    key.Value = Config.defaultDoc.Root.Element(key.Name).Value;
                    result = bool.Parse(key.Value);
                    return result;
                }
            }
            set { Config.doc.Root.Element("IsFieldsInvalidHold").Value = value.ToString(); }
        }
        public static bool IsFilesInvalidDelete
        {
            get
            {
                XElement key = Config.doc.Root.Element("IsFilesInvalidDelete");
                bool result;
                if (bool.TryParse(key.Value, out result))
                    return result;
                else
                {
                    key.Value = Config.defaultDoc.Root.Element(key.Name).Value;
                    result = bool.Parse(key.Value);
                    return result;
                }
            }
            set { Config.doc.Root.Element("IsFilesInvalidDelete").Value = value.ToString(); }
        }
        public static bool IsFoldersEmptyDelete
        {
            get
            {
                XElement key = Config.doc.Root.Element("IsFoldersEmptyDelete");
                bool result;
                if (bool.TryParse(key.Value, out result))
                    return result;
                else
                {
                    key.Value = Config.defaultDoc.Root.Element(key.Name).Value;
                    result = bool.Parse(key.Value);
                    return result;
                }
            }
            set { Config.doc.Root.Element("IsFoldersEmptyDelete").Value = value.ToString(); }
        }

        #endregion

    }
}
