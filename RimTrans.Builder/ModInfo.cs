using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RimTrans.Builder
{
    public class ModInfo
    {
        public ModInfo(string path)
        {
            this._rootDir = new DirectoryInfo(path);
            this._aboutDir = new DirectoryInfo(Path.Combine(path, "About"));
            this._defsDir = new DirectoryInfo(Path.Combine(path, "Defs"));
            this._languagesDir = new DirectoryInfo(Path.Combine(path, "Languages"));
            this._defaultLanguagePath = (Path.Combine(Path.Combine(path, "Languages"), "English"));
        }

        private DirectoryInfo _rootDir;
        public DirectoryInfo RootDir
        {
            get
            {
                return _rootDir;
            }
        }

        private DirectoryInfo _aboutDir;
        public DirectoryInfo AboutDir
        {
            get
            {
                return this._aboutDir;
            }
        }

        private DirectoryInfo _defsDir;
        public DirectoryInfo DefsDir
        {
            get
            {
                return this._defsDir;
            }
        }

        private DirectoryInfo _languagesDir;
        public DirectoryInfo LanguagesDir
        {
            get
            {
                return this._languagesDir;
            }
        }

        private string _defaultLanguagePath;
        public string DefaultLanguagePath
        {
            get
            {
                return this._defaultLanguagePath;
            }
        }

        public override string ToString()
        {
            string text = this._rootDir.FullName + "\n"
                + this._aboutDir.FullName + "\n"
                + this._defsDir.FullName + "\n"
                + this._languagesDir.FullName + "\n";
            return text;
        }
    }
}
