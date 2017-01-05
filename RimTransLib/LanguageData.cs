using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RimTransLib
{
    public class LanguageData : ILanguageData<LanguageData>
    {
        private LanguageData(LanguageInfo languageInfo)
        {
            this._languageInfo = languageInfo;
        }

        public LanguageData(LanguageInfo languageInfo, DefinitionData definitions, bool isCore = false)
        {

        }

        #region Info

        private LanguageInfo _languageInfo;
        public LanguageInfo LanguageInfo
        {
            get
            {
                return this._languageInfo;
            }
        }

        #endregion

        #region Data

        private InjectionData _injection;
        public InjectionData Injection
        {
            get
            {
                return this._injection;
            }
        }

        private KeyData _key;
        public KeyData Key
        {
            get
            {
                return this._key;
            }
        }

        private StringData _string;
        public StringData String
        {
            get
            {
                return this._string;
            }
        }

        #endregion

        #region Loader and Parser

        public static LanguageData Load(LanguageInfo languageInfo)
        {
            LanguageData language = new LanguageData(languageInfo);
            language._injection = InjectionData.Load(languageInfo.DefInjectedDir);
            language._key = KeyData.Load(languageInfo.KeyedDir);
            language._string = StringData.Load(languageInfo.StringsDir);
            return language;
        }

        public static LanguageData Parse(DefinitionData definition, LanguageInfo defaultlanguageInfo, bool isCore)
        {
            LanguageData language = new LanguageData(defaultlanguageInfo);
            language._injection = InjectionData.Parse(definition, defaultlanguageInfo.DefInjectedDir, isCore);
            language._key = KeyData.Load(defaultlanguageInfo.KeyedDir);
            language._string = StringData.Load(defaultlanguageInfo.StringsDir);
            return language;
        }

        #endregion

        #region Clear

        public void Empty()
        {
            if (this._languageInfo.DefInjectedDir.Exists)
            {
                DeleteDirectoryRecursively(this._languageInfo.DefInjectedDir, "*.xml");
            }
            if (this._languageInfo.KeyedDir.Exists)
            {
                DeleteDirectoryRecursively(this._languageInfo.KeyedDir, "*.xml");
            }
            if (this._languageInfo.StringsDir.Exists)
            {
                DeleteDirectoryRecursively(this._languageInfo.StringsDir, "*.txt");
            }
        }

        private static void DeleteDirectoryRecursively(DirectoryInfo dir, string fileSearchPattern = "*")
        {
            foreach (FileInfo fileInfo in dir.GetFiles(fileSearchPattern, SearchOption.TopDirectoryOnly))
            {
                fileInfo.Delete();
            }
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                DeleteDirectoryRecursively(subDir);
            }
            try
            {
                dir.Delete();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Interface

        public LanguageData BuildNew(LanguageData languageOriginial, bool isRebuild, LanguageData languageCore)
        {
            LanguageData language = new LanguageData(this._languageInfo);
            if (languageCore == null)
            {
                language._injection = this._injection.BuildNew(languageOriginial._injection, isRebuild);
                language._key = this._key.BuildNew(languageOriginial._key, isRebuild);
                language._string = this._string.BuildNew(isRebuild);
            }
            else
            {
                language._injection = this._injection.BuildNew(languageOriginial._injection, isRebuild, languageCore._injection);
                language._key = this._key.BuildNew(languageOriginial._key, isRebuild, languageCore._key);
                language._string = this._string.BuildNew(isRebuild);
            }
            return language;
        }

        public void Save()
        {
            this._injection.Save();
            this._key.Save();
            this._string.Save();
        }

        public void Save(string path)
        {
            this._injection.Save(Path.Combine(path, "Injected"));
            this._key.Save(Path.Combine(path, "Keyed"));
            this._string.Save(Path.Combine(path, "Strings"));
        }

        #endregion
    }
}
