using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RimTransLib
{
    public class ModData
    {
        public ModData(ModInfo modInfo, List<LanguageInfo> languageInfos, bool isCore, ModData core = null)
        {
            this._modInfo = modInfo;
            this._languageInfos = new List<LanguageInfo>();
            foreach (LanguageInfo languageInfo in languageInfos)
            {
                LanguageInfo instantiate = languageInfo.Instantiate(this._modInfo.LanguagesDir);
                this._languageInfos.Add(instantiate);
            }
            this._isCore = isCore;
            this._core = core;
        }

        #region Info

        private ModInfo _modInfo;
        public ModInfo Info
        {
            get
            {
                return this._modInfo;
            }
        }

        private List<LanguageInfo> _languageInfos;
        public List<LanguageInfo> LanguageInfos
        {
            get
            {
                return this._languageInfos;
            }
        }

        private bool _isCore;
        public bool IsCore
        {
            get
            {
                return this._isCore;
            }
        }
        private ModData _core;

        #endregion

        #region Data

        private DefinitionData _definition;
        public DefinitionData Definition
        {
            get
            {
                if (this._definition == null)
                {
                    if (this._isCore || this._core == null)
                    {
                        this._definition = DefinitionData.Load(this._modInfo.DefsDir);
                    }
                    else
                    {
                        this._definition = DefinitionData.Load(this._modInfo.DefsDir, this._core.Definition);
                    }
                }
                return this._definition;
            }
        }

        private LanguageData _languageOriginal;
        public LanguageData LanguageOriginal
        {
            get
            {
                if (this._languageOriginal == null)
                {
                    LanguageInfo languageInfoDefault = new LanguageInfo("English", "Englsh", this._modInfo.DefaultLanguagePath);
                    this._languageOriginal = LanguageData.Parse(this.Definition, languageInfoDefault, this._isCore);
                }
                return this._languageOriginal;
            }
        }

        private List<LanguageData> _languagesExisting;
        public List<LanguageData> LanguagesExisting
        {
            get
            {
                if (this._languagesExisting == null)
                {
                    this._languagesExisting = new List<LanguageData>();
                    foreach (LanguageInfo languageInfo in this._languageInfos)
                    {
                        this._languagesExisting.Add(LanguageData.Load(languageInfo));
                    }
                }
                return this._languagesExisting;
            }
        }

        /// <summary>
        /// Get a LanguagData object by language name
        /// </summary>
        public LanguageData Language(string languageName)
        {
            foreach (LanguageData language in this.LanguagesExisting)
            {
                if (language.LanguageInfo.Name == languageName)
                {
                    return language;
                }
            }
            return null;
        }

        #endregion

        #region Build Language Data

        public void BuildLanguageData(bool isRebuild = false)
        {
            if (!this._isCore && this._core != null)
            {
                TransLog.Message(this, new TransLog.MessageArgs(
                TransLog.Type.Message,
                "Loading the Core languages...",
                null));
                if (this._core.LanguagesExisting == null)
                {
                    //TODO: ???
                }
            }

            TransLog.Message(this, new TransLog.MessageArgs(
                TransLog.Type.Message,
                "Loading the existing languages...",
                null));
            if (this.LanguagesExisting == null)
            {
                //TODO: ???
            }

            TransLog.Message(this, new TransLog.MessageArgs(
                TransLog.Type.Message,
                "Begin to construct original language...",
                null));
            LanguageData languageOriginal = this.LanguageOriginal;
            TransLog.Message(this, new TransLog.MessageArgs(
                TransLog.Type.Message,
                "Construction completed.",
                null));

            foreach (LanguageData languageExisting in this.LanguagesExisting)
            {
                TransLog.Message(this, new TransLog.MessageArgs(
                    TransLog.Type.Message,
                    "Begin to construct language: " + languageExisting.LanguageInfo.Name + "(" + languageExisting.LanguageInfo.NameNative + ")...",
                    null));
                LanguageData languageCore = null;
                if (!this._isCore && this._core != null)
                {
                    languageCore = this._core.Language(languageExisting.LanguageInfo.Name);
                }
                LanguageData languageNew = languageExisting.BuildNew(languageOriginal, isRebuild, languageCore);
                if (isRebuild)
                {
                    try
                    {
                        languageNew.Empty();
                    }
                    catch (Exception ex)
                    {
                        TransLog.Message(this, new TransLog.MessageArgs(
                            TransLog.Type.Error,
                            "BuildLanguageData.Rebuild Delete failed!",
                            ex.Message));
                    }
                    finally
                    {
                        TransLog.Message(this, new TransLog.MessageArgs(
                            TransLog.Type.Message,
                            "Delete all existing language data...",
                            null));
                    }
                }
                try
                {
                    languageNew.Save();
                }
                catch (Exception ex)
                {
                    TransLog.Message(this, new TransLog.MessageArgs(
                        TransLog.Type.Error,
                        "BuildLanguageData Save failed!",
                        ex.Message));
                }
                finally
                {
                    TransLog.Message(this, new TransLog.MessageArgs(
                        TransLog.Type.Message,
                        "Construction completed: " + languageExisting.LanguageInfo.Name,
                        "Output directory: " + languageNew.LanguageInfo.RootDir.FullName));
                }
            }
        }

        #endregion


    }
}
