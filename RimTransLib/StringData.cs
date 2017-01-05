using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RimTransLib
{
    public class StringData : ILanguageData<StringData>
    {
        private StringData(DirectoryInfo stringDir)
        {
            this._rootDir = stringDir;
        }

        #region Info

        private DirectoryInfo _rootDir;
        public DirectoryInfo RootDir
        {
            get
            {
                return _rootDir;
            }
        }

        #endregion

        #region Data

        private SortedDictionary<string, string> _dataBase;
        public SortedDictionary<string, string> DataBase
        {
            get
            {
                return this._dataBase;
            }
        }

        #endregion

        #region Loader

        public static StringData Load(DirectoryInfo stringDir)
        {
            StringData stringData = new StringData(stringDir);

            stringData._dataBase = new SortedDictionary<string, string>();
            int splitIndex = stringData._rootDir.FullName.Length;
            foreach (FileInfo fileInfo in stringData._rootDir.GetFiles("*.txt", SearchOption.AllDirectories))
            {
                string key = fileInfo.FullName.Substring(splitIndex);
                string text = string.Empty;
                using (StreamReader sr = new StreamReader(fileInfo.FullName))
                {
                    text = sr.ReadToEnd();
                }
                stringData._dataBase.Add(key, text);
            }

            return stringData;
        }

        #endregion

        #region Interface

        public StringData BuildNew(bool isRebuild)
        {
            StringData stringData = new StringData(this._rootDir);

            stringData._dataBase = new SortedDictionary<string, string>();
            foreach (KeyValuePair<string, string> kvpRelativPathText in this._dataBase)
            {
                string text = kvpRelativPathText.Value;
                stringData._dataBase.Add(kvpRelativPathText.Key, text);
            }

            return stringData;
        }

        public StringData BuildNew(StringData stringOriginal, bool isRebuild, StringData stringCore = null)
        {
            return this.BuildNew(isRebuild);
        }

        public void Save()
        {
            this.Save(this._rootDir.FullName);
        }

        public void Save(string path)
        {
            foreach (KeyValuePair<string, string> kvpRelativePathText in this._dataBase)
            {
                string filePath = path + kvpRelativePathText.Key;
                FileInfo fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    DirectoryInfo dir = fileInfo.Directory;
                    if (!dir.Exists) dir.Create();

                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        sw.Write(kvpRelativePathText.Value);
                    }
                }
            }
        }

        #endregion
    }
}
