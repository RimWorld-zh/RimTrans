using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RimTransLib
{
    public class KeyData : ILanguageData<KeyData>
    {
        private KeyData(DirectoryInfo keyDir)
        {
            this._rootDir = keyDir;
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

        private SortedDictionary<string, XDocument> _dataBase;
        public SortedDictionary<string, XDocument> DataBase
        {
            get
            {
                return this._dataBase;
            }
        }

        #endregion

        #region Loader

        public static KeyData Load(DirectoryInfo keyDir)
        {
            KeyData keyData = new KeyData(keyDir);

            keyData._dataBase = new SortedDictionary<string, XDocument>();
            int splitIndex = keyData._rootDir.FullName.Length;
            if (keyData._rootDir.Exists)
            {
                foreach (FileInfo fileInfo in keyData._rootDir.GetFiles("*.xml", SearchOption.AllDirectories))
                {
                    try
                    {
                        string key = fileInfo.FullName.Substring(splitIndex);
                        XDocument doc = RWXml.LoadLanguageDocument(fileInfo.FullName);
                        keyData._dataBase.Add(key, doc);
                    }
                    catch (XmlException ex)
                    {
                        //TODO: log
                    }
                }
            }

            return keyData;
        }

        #endregion

        #region Matcher

        private void MatchExisting(KeyData keyExisting)
        {
            foreach (XElement keyedThis in from doc in this._dataBase.Values
                                           from ele in doc.Root.Elements()
                                           select ele)
            {
                foreach (XElement keyedExisting in from doc in keyExisting._dataBase.Values
                                                   from ele in doc.Root.Elements()
                                                   select ele)
                {
                    if (keyedThis.Name == keyedExisting.Name)
                    {
                        keyedThis.Value = keyedExisting.Value;
                        break;
                    }
                }
            }
            foreach (KeyValuePair<string, XDocument> kvpRelativePathDocumentExisting in keyExisting._dataBase)
            {
                XDocument docExisting = kvpRelativePathDocumentExisting.Value;
                XDocument docThis;
                if (this._dataBase.TryGetValue(kvpRelativePathDocumentExisting.Key, out docThis))
                {
                    int countInvalid = 0;
                    foreach (XNode nodeExiting in docExisting.Root.Nodes())
                    {
                        if (nodeExiting.NodeType == XmlNodeType.Comment)
                        {
                            bool isInvalid = true;
                            foreach (XNode nodeThis in docThis.Root.Nodes())
                            {
                                if (nodeThis.NodeType == XmlNodeType.Comment &&
                                    (nodeExiting as XComment).Value == (nodeThis as XComment).Value)
                                {
                                    isInvalid = false;
                                    break;
                                }
                            }
                            if (isInvalid)
                            {
                                if (countInvalid == 0)
                                {
                                    docThis.Root.Add("\r\n");
                                }
                                docThis.Root.Add("  ", nodeExiting, "\r\n");
                                countInvalid++;
                            }
                        }
                        else if (nodeExiting.NodeType == XmlNodeType.Element)
                        {
                            bool isInvalid = true;
                            if (docThis.Root.Element((nodeExiting as XElement).Name) != null)
                            {
                                isInvalid = false;
                            }
                            if (isInvalid)
                            {
                                if (countInvalid == 0)
                                {
                                    docThis.Root.Add("\r\n");
                                }
                                docThis.Root.Add("  ", new XComment((nodeExiting as XElement).ToString()), "\r\n");
                                countInvalid++;
                            }
                        }
                    }
                    if (countInvalid > 0)
                    {
                        docThis.Root.Add("\r\n");
                    }
                }
                else
                {
                    this._dataBase.Add(kvpRelativePathDocumentExisting.Key, RWXml.DocumentCommentAll(kvpRelativePathDocumentExisting.Value));
                }
            }
        }

        private void MatchSelf()
        {
            List<XElement> keyeds = new List<XElement>();
            keyeds.AddRange(from doc in this._dataBase.Values
                            from ele in doc.Root.Elements()
                            select ele);
            foreach (XElement keyed in keyeds)
            {
                IEnumerable<XElement> keyedsGatherIE = from doc in this._dataBase.Values
                                                      from ele in doc.Root.Elements(keyed.Name)
                                                      select ele;
                if (keyedsGatherIE.Count() > 1)
                {
                    List<XElement> keyedsGatherList = new List<XElement>();
                    keyedsGatherList.AddRange(keyedsGatherIE);
                    for (int i = 1; i < keyedsGatherList.Count; i++)
                    {
                        keyedsGatherList[i].ReplaceWith(new XComment(string.Format("[Duplicate] {0}", keyedsGatherList[i].ToString())));
                    }
                }
            }
        }

        private void MatchCore(KeyData keyCore)
        {
            List<XElement> keyedsConficted = new List<XElement>();
            foreach (XElement keyedThis in from doc in this._dataBase.Values
                                           from ele in doc.Root.Elements()
                                           select ele)
            {
                foreach (XElement keyedCore in from doc in keyCore._dataBase.Values
                                               from ele in doc.Root.Elements()
                                               select ele)
                {
                    if (keyedThis.Name == keyedCore.Name)
                    {
                        keyedsConficted.Add(keyedThis);
                    }
                }
            }
            foreach (XElement keyed in keyedsConficted)
            {
                keyed.ReplaceWith(new XComment(string.Format("[Core] {0}", keyed.ToString())));
            }
        }

        #endregion

        #region Interface

        public KeyData BuildNew(KeyData keyOriginal, bool isRebuild, KeyData keyCore = null)
        {
            KeyData keyNew = new KeyData(this._rootDir);

            keyNew._dataBase = new SortedDictionary<string, XDocument>();
            foreach (KeyValuePair<string, XDocument> kvpRelativePathDocument in keyOriginal._dataBase)
            {
                keyNew._dataBase.Add(kvpRelativePathDocument.Key, new XDocument(kvpRelativePathDocument.Value));
            }
            if (!isRebuild)
            {
                keyNew.MatchExisting(this);
            }
            keyNew.MatchSelf();
            if (keyCore != null)
            {
                keyNew.MatchCore(keyCore);
            }

            return keyNew;
        }

        public void Save()
        {
            this.Save(this._rootDir.FullName);
        }

        public void Save(string path)
        {
            foreach (KeyValuePair<string, XDocument> kvpRelativePathDocument in this._dataBase)
            {
                string filePath = path + kvpRelativePathDocument.Key;
                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(filePath));
                if (!dir.Exists) dir.Create();

                kvpRelativePathDocument.Value.Save(filePath);
            }
        }

        #endregion
    }
}
