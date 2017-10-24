using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RimWorld;
using Verse;
using UnityEngine;

using RimTrans.Builder.Xml;

namespace RimTrans.Builder.Crawler {
    /// <summary>
    /// Storage Def info, included tags and fields.
    /// </summary>
    public class DefInfo {
        #region Fields

        /// <summary>
        /// DefType Name of this DefType
        /// </summary>
        public string Name { get { return this.defTypeName; } }
        private string defTypeName;


        /// <summary>
        /// Get type of this DefType
        /// </summary>
        public Type DefType { get { return this.defType; } }
        private Type defType;


        /// <summary>
        /// Get all fields of this type of Def.
        /// </summary>
        public IEnumerable<FieldInfo> Fields { get { return this.allFields.Values; } }
        private Dictionary<string, FieldInfo> allFields = new Dictionary<string, FieldInfo>();


        /// <summary>
        /// Get all Tags of this type of Def.
        /// </summary>
        public IEnumerable<TagInfo> Tags { get { return this.allTags.Values; } }
        private Dictionary<string, TagInfo> allTags;

        /// <summary>
        /// If this Def has tags.
        /// </summary>
        public bool HasTags { get { return this.allTags != null && this.allTags.Count > 0; } }


        /// <summary>
        /// Non-matched elements.
        /// </summary>
        public IEnumerable<XElement> NonMatchedElements { get { return this.nonMatchedElements; } }
        private List<XElement> nonMatchedElements = new List<XElement>();

        /// <summary>
        /// If this Def has non-matched elements.
        /// </summary>
        public bool HasNonMatched { get { return this.nonMatchedElements != null && this.nonMatchedElements.Count > 0; } }

        public bool IsValid { get { return this.isValid; } }
        private bool isValid = true;

        #endregion

        #region Init

        /// <summary>
        /// Initialize a DefInfo from xml element.
        /// </summary>
        /// <param name="def">The xml element of this type of Def.</param>
        public DefInfo(XElement defElement) {
            this.allTags = new Dictionary<string, TagInfo>();
            XElement def = new XElement(defElement);

            this.defTypeName = def.Name.ToString();
            foreach (Type curDefType in Helper.AllClassesDef) {
                if (defTypeName == curDefType.Name) {
                    this.defType = curDefType;
                }
            }

            if (this.defType == null) {
                Log.Error();
                Log.WriteLine($"ERROR: defType '{defTypeName}' no found.");
                this.isValid = false;
                //Log.WriteLine(def.ToString());
                return;
            }

            LinkedList<Type> types = new LinkedList<Type>();
            {
                types.AddFirst(this.defType);
                Type curType = this.defType;
                while (curType != Helper.ClassDef) {
                    curType = curType.BaseType;
                    types.AddFirst(curType);
                }
            }
            foreach (Type curType in types) {
                foreach (FieldInfo curFieldInfo in curType.GetFields(Helper.FieldBindingFlags)) {
                    if (!this.allFields.ContainsKey(curFieldInfo.Name)) {
                        this.allFields.Add(curFieldInfo.Name, curFieldInfo);
                    }
                }
            }

            List<XElement> matchedElements = new List<XElement>();
            foreach (var kvpNameFieldInfo in this.allFields) {
                string curName = kvpNameFieldInfo.Key;
                FieldInfo curFieldInfo = kvpNameFieldInfo.Value;
                string curAlias = Helper.GetAlias(curFieldInfo);

                List<XElement> aliasElements = new List<XElement>();
                foreach (XElement curAliasElememt in def.Elements()) {
                    if (string.Compare(curAliasElememt.Name.ToString(), curAlias, true) == 0) {
                        bool isMatched = false;
                        foreach (XElement curElement in def.Elements()) {
                            if (string.Compare(curElement.Name.ToString(), curName, true) == 0 &&
                                curAliasElememt.GetClassName() == curElement.GetClassName()) {
                                isMatched = true;
                                Helper.MergeElement(curElement, curAliasElememt);
                                Helper.MergeListItems(curElement);
                                aliasElements.Add(curAliasElememt);
                                break;
                            }
                        }
                        if (!isMatched) {
                            curAliasElememt.Name = curName;
                        }
                    }
                }
                foreach (XElement curAliasElement in aliasElements) {
                    curAliasElement.Remove();
                }

                foreach (XElement curElement in def.Elements()) {
                    if (string.Compare(curElement.Name.ToString(), curName, true) == 0) {
                        string className = curElement.GetClassName();
                        if (className == null) {
                            string key = curName;
                            if (!this.allTags.ContainsKey(key)) {
                                this.allTags.Add(key, new TagInfo(this, curElement, curFieldInfo));
                                matchedElements.Add(curElement);
                            }
                        } else {
                            IEnumerable<Type> allSubClasses = curFieldInfo.FieldType.AllSubclasses();
                            foreach (Type curSubClass in allSubClasses) {
                                if (className == curSubClass.Name) {
                                    string key = $"{curName} {className}";
                                    if (!this.allTags.ContainsKey(key)) {
                                        this.allTags.Add(key, new TagInfo(this, curElement, curFieldInfo, curSubClass));
                                        matchedElements.Add(curElement);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            foreach (XElement curElement in def.Elements()) {
                if (!matchedElements.Contains(curElement)) {
                    Log.Warning();
                    Log.WriteLine($"The element '{curElement.Name.ToString()}' of {this.defTypeName} no matched.");
                    this.nonMatchedElements.Add(curElement);
                }
            }
        }

        #endregion


        #region ToXElemnet

        public XElement ToXElement() {
            XElement def = new XElement(this.defTypeName);
            foreach (TagInfo tag in this.allTags.Values) {
                def.Add(tag.ToXNodes());
            }
            if (this.HasNonMatched) {
                def.Add(new XComment("======== Non Matched Elements ========"));
                def.Add(this.nonMatchedElements);
            }
            return def;
        }

        #endregion


        #region ProcessFieldNames

        /// <summary>
        /// Process case and alias of all fields in this definition data.
        /// </summary>
        /// <param name="def"></param>
        public void ProcessFieldNames(XElement def) {
            if (def.Name.ToString() != this.defTypeName)
                throw new ArgumentException($"DefType '{def.Name.ToString()}' and DefInfo '{this.defTypeName}' no matched.", "def");

            if (!this.HasTags) {
                Log.Warning();
                Log.WriteLine($"Process field names failure for '{def.Name.ToString()}': DefInfo '{this.defTypeName}' has no tags.");
                return;
            }

            var allTagInfos = this.allTags.Values;
            List<XElement> matchedTags = new List<XElement>();
            Dictionary<XElement, XComment> bufferComments = new Dictionary<XElement, XComment>();
            foreach (TagInfo curTagInfo in allTagInfos) {
                string curName = curTagInfo.Name;
                string curAlias = curTagInfo.Alias;
                foreach (XElement curTag in def.Elements()) {
                    string curTagName = curTag.Name.ToString();
                    if ((string.Compare(curTagName, curName, true) == 0 ||
                        string.Compare(curTagName, curAlias, true) == 0) &&
                        curTag.GetClassName() == curTagInfo.ClassName) {
                        XNode node = curTag.PreviousNode;
                        if (node != null && node.NodeType == XmlNodeType.Comment) {
                            node.Remove();
                            bufferComments.Add(curTag, node as XComment);
                        }
                        curTagInfo.ProcessFieldNames(curTag);
                        curTag.Remove();
                        matchedTags.Add(curTag);
                        break;
                    }
                }
            }
            List<XElement> nonMatchedElements = new List<XElement>();
            if (def.HasElements) {
                foreach (XElement curTag in def.Elements()) {
                    Log.Warning();
                    Log.WriteLine($"Tag <{curTag.Name.ToString()}> no matched field. In {def.BaseInfo()}.");
                }
                nonMatchedElements.AddRange(def.Elements());
                def.RemoveNodes();
            }
            foreach (XElement curTag in matchedTags) {
                XComment curComment;
                if (bufferComments.TryGetValue(curTag, out curComment)) {
                    def.Add(curComment);
                }
                def.Add(curTag);
            }
            def.Add(nonMatchedElements);
        }

        #endregion
    }
}
