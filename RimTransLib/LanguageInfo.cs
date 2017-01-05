using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RimTransLib
{
    public class LanguageInfo
    {
        public LanguageInfo(string name, string nameNative, string customPath = null)
        {
            this._name = name;
            this._nameNative = nameNative;
            if (customPath != null && customPath != string.Empty)
            {
                this._customPath = customPath;
                this._rootDir = new DirectoryInfo(this._customPath);
                this._defInjectedDir = new DirectoryInfo(Path.Combine( this._customPath, "DefInjected"));
                this._keyedDir = new DirectoryInfo(Path.Combine(this._customPath, "Keyed"));
                this._stringsDir = new DirectoryInfo(Path.Combine(this._customPath, "Strings"));
            }
            else
            {
                this._customPath = string.Empty;
            }
        }

        /// <summary>
        /// the true name, also the folder name of this language
        /// </summary>
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// the native name, for showing
        /// </summary>
        private string _nameNative;
        public string NameNative
        {
            get
            {
                return _nameNative;
            }
        }

        private string _customPath;
        public string CustomPath
        {
            get
            {
                return _customPath;
            }
        }

        private DirectoryInfo _rootDir;
        public DirectoryInfo RootDir
        {
            get
            {
                return _rootDir;
            }
        }

        private DirectoryInfo _defInjectedDir;
        public DirectoryInfo DefInjectedDir
        {
            get
            {
                return _defInjectedDir;
            }
        }

        private DirectoryInfo _keyedDir;
        public DirectoryInfo KeyedDir
        {
            get
            {
                return _keyedDir;
            }
        }

        private DirectoryInfo _stringsDir;
        public DirectoryInfo StringsDir
        {
            get
            {
                return _stringsDir;
            }
        }

        /// <summary>
        /// Convert a generic LanguageInfo object to a special LanguageInfo object for ModData
        /// </summary>
        /// <param name="LanguagesDir"></param>
        /// <returns></returns>
        public LanguageInfo Instantiate(DirectoryInfo LanguagesDir)
        {
            string customPath;
            if (this._customPath == null && this._customPath == string.Empty)
            {
                customPath = Path.Combine(LanguagesDir.FullName, this._name);
            }
            else
            {
                customPath = this._customPath;
            }
            return new LanguageInfo(this._name, this._nameNative, customPath);
        }

        public override string ToString()
        {
            string text = this._name + "\n"
                + this._rootDir.FullName + "\n"
                + this._defInjectedDir.FullName + "\n"
                + this._keyedDir.FullName + "\n"
                + this._stringsDir.FullName + "\n";
            return text;
        }
    }
}
