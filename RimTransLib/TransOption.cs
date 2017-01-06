using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RimTransLib
{
    public static class TransOption
    {
        public static void Initial(string corePath)
        {
            DirectoryInfo coreDir = new DirectoryInfo(corePath);
            if (coreDir.Exists)
            {
                string languagesPath = Path.Combine(corePath, "Languages");
                DirectoryInfo languagesDir = new DirectoryInfo(languagesPath);
                if (languagesDir.Exists)
                {
                    foreach (DirectoryInfo langDir in languagesDir.GetDirectories())
                    {
                        string name = langDir.Name;
                        string nameNative = name;
                        try
                        {
                            XDocument doc = XDocument.Load(Path.Combine(langDir.FullName, "LanguageInfo.xml"));
                            nameNative = doc.Root.Element("friendlyNameNative").Value;
                        }
                        catch (Exception)
                        {
                        }
                        _supportLanguages.Add(new LanguageInfo(name, nameNative));
                    }
                    Core = new ModData(new ModInfo(corePath), _supportLanguages, true);
                }
            }
            else
            {
                Core = null;
            }
        }

        public static ModData Core { get; private set; }

        private static List<LanguageInfo> _supportLanguages = new List<LanguageInfo>();
        public static IEnumerable<LanguageInfo> SupportLanguages
        {
            get
            {
                return _supportLanguages;
            }
        }

        private static bool _isRebuild = false;
        public static bool IsRebuild
        {
            get
            {
                return _isRebuild;
            }
            set
            {
                _isRebuild = value;
            }
        }

        private static bool _replaceIndent = true;
        public static bool IsReplaceIndent
        {
            get
            {
                return _replaceIndent;
            }
            set
            {
                _replaceIndent = value;
            }
        }

        private static string _indent = "  ";
        public static string Indent
        {
            get
            {
                return _indent;
            }
        }

        private static int _indentNumber = 2;
        public static int IndentNumber
        {
            get
            {
                return _indentNumber;
            }
            set
            {
                if (value < 0)
                {
                    _indentNumber = 0;
                }
                else if (value <= 10)
                {
                    _indentNumber = value;
                }
                else
                {
                    _indentNumber = 2;
                }
                switch (_indentNumber)
                {
                    case 1:
                        _indent = " ";
                        break;
                    case 2:
                        _indent = "  ";
                        break;
                    case 3:
                        _indent = "   ";
                        break;
                    case 4:
                        _indent = "    ";
                        break;
                    case 5:
                        _indent = "     ";
                        break;
                    case 6:
                        _indent = "      ";
                        break;
                    case 7:
                        _indent = "       ";
                        break;
                    case 8:
                        _indent = "        ";
                        break;
                    case 9:
                        _indent = "         ";
                        break;
                    case 10:
                        _indent = "          ";
                        break;
                    default:
                        _indent = string.Empty;
                        break;
                }
            }
        }

        private static string _currentLanguage = "English";
        public static string CurrentLanguage
        {
            get
            {
                return _currentLanguage;
            }
            set
            {
                _currentLanguage = value;
            }
        }


    }
}
