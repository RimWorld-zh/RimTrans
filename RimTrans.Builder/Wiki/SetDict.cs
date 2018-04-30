using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using RimTrans.Builder.Xml;
using RimTrans.Builder.Crawler;

namespace RimTrans.Builder.Wiki {
    public class SetDict {
        private static readonly UTF8Encoding utf8 = new UTF8Encoding(false);

        private string defType;
        public string DefType { get { return this.defType; } }

        private string defName;
        public string DefName { get { return this.defName; } }

        private Dictionary<string, string> dict;
        public Dictionary<string, string> Dict { get { return this.dict; } }


        #region Init

        /// <summary>
        /// Create SetDict frome def element and def info.
        /// </summary>
        /// <param name="defElement"></param>
        /// <param name="defInfo"></param>
        public SetDict(XElement defElement, DefInfo defInfo) {
            if (defElement == null)
                throw new ArgumentNullException("defElement");

            if (defInfo == null)
                throw new ArgumentNullException("defInfo");

            this.defType = defElement.Name.ToString();
            this.defName =
                defElement.Element("defName") == null ?
                "UnnamedDef" :
                defElement.Element("defName").Value;
            this.dict = new Dictionary<string, string>();

            LinkedList<XElement> linkedElements = new LinkedList<XElement>();
            linkedElements.AddLast(defElement);

            foreach (TagInfo curTagInfo in defInfo.Tags) {
                string curName = curTagInfo.Name;
                string curClassName = curTagInfo.ClassName;
                bool isMatched = false;
                foreach (XElement curTagElement in defElement.Elements()) {
                    if (curTagElement.Name.ToString() == curName &&
                        curTagElement.GetClassName() == curClassName) {
                        isMatched = true;
                        linkedElements.AddLast(curTagElement);
                        this.AddMatched(linkedElements, curTagInfo);
                        linkedElements.RemoveLast();
                    }
                }
                if (!isMatched) {
                    this.AddNonMatched(curTagInfo);
                }
            }
        }

        /// <summary>
        /// Add key value pair to dict.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value) {
            value = value.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\\n", "<br/>").Replace("|", "{{!}}");
            if (this.dict.ContainsKey(key)) {
                this.dict[key] = value;
                Log.Warning();
                Log.WriteLine($"Duplicated key: key='{key}', value='{value}'.");
            } else {
                this.dict.Add(key, value);
            }
        }

        public void AddClass(LinkedList<XElement> linkedElements) {
            string className = linkedElements.Last.Value.GetClassName();
            if (className != null) {
                this.Add(LinkedToKey(linkedElements) + ".Class", className);
            }
        }

        private void AddNonMatched(TagInfo tagInfo) {

        }

        private void AddMatched(LinkedList<XElement> linkedElements, TagInfo tagInfo) {
            switch (tagInfo.Category) {
            case TagCategory.Unkown:
                Log.Error();
                Log.WriteLine($"Unkown tag '{linkedElements.Last.Value.Name.ToString()}'");
                break;
            case TagCategory.Standard:
            case TagCategory.ListComplexItem: {
                    XElement tagElement = linkedElements.Last.Value;
                    if (tagElement.HasElements) {
                        if (tagInfo.HasTags) {
                            if (this.CanBeRange(linkedElements, tagInfo)) {

                            } else {
                                this.Add(LinkedToKey(linkedElements), "Exist");
                                this.AddClass(linkedElements);
                                foreach (TagInfo curSubTagInfo in tagInfo.Tags) {
                                    string curSubName = curSubTagInfo.Name;
                                    string curSubClassName = curSubTagInfo.ClassName;
                                    bool isMatched = false;
                                    foreach (XElement curSubTagElement in tagElement.Elements()) {
                                        if (curSubTagElement.Name.ToString() == curSubName &&
                                            curSubTagElement.GetClassName() == curSubClassName) {
                                            isMatched = true;
                                            linkedElements.AddLast(curSubTagElement);
                                            this.AddMatched(linkedElements, curSubTagInfo);
                                            linkedElements.RemoveLast();
                                        }
                                    }
                                    if (!isMatched) {
                                        this.AddNonMatched(curSubTagInfo);
                                    }
                                }
                            }
                        } else {
                            Log.Warning();
                            Log.WriteLine($"Tag '{LinkedToKey(linkedElements)}' has sub-elements, but matched TagInfo that has no sub-tags.");
                        }
                    } else {
                        if (tagInfo.Category == TagCategory.Standard) {
                            this.Add(LinkedToKey(linkedElements), linkedElements.Last.Value.Value);
                        } else if (tagInfo.Category == TagCategory.ListComplexItem) {
                            this.Add(LinkedToKey(linkedElements), "Exist");
                            this.AddClass(linkedElements);
                        }
                    }
                }
                break;
            case TagCategory.ListComplex: {
                    if (this.CanBeListRef(linkedElements, tagInfo)) {

                    } else if (this.CanBeListRefRange(linkedElements, tagInfo)) {

                    } else {
                        XElement tagElement = linkedElements.Last.Value;
                        this.Add(LinkedToKey(linkedElements), "Exist");
                        this.AddClass(linkedElements);
                        if (tagElement.HasElements) {
                            this.Add(LinkedToKey(linkedElements, true), tagElement.Elements().Count().ToString());
                            if (tagInfo.HasTags) {
                                foreach (XElement curItemElement in tagElement.Elements()) {
                                    foreach (TagInfo curSubTagInfo in tagInfo.Tags) {
                                        if (curItemElement.GetClassName() == curSubTagInfo.ClassName) {
                                            linkedElements.AddLast(curItemElement);
                                            this.AddMatched(linkedElements, curSubTagInfo);
                                            linkedElements.RemoveLast();
                                            break;
                                        }
                                    }
                                }
                            } else {
                                Log.Warning();
                                Log.WriteLine($"Tag '{LinkedToKey(linkedElements)}' has sub-elements, but matched TagInfo that has no sub-tags.");
                            }
                        } else {
                            this.Add(LinkedToKey(linkedElements, true), "0");
                        }
                    }
                }
                break;
            case TagCategory.Def:
            case TagCategory.Enum: {
                    this.Add(LinkedToKey(linkedElements), linkedElements.Last.Value.Value);
                }
                break;
            case TagCategory.Flag:
            case TagCategory.ListSimple:
            case TagCategory.ListDef: {
                    StringBuilder sb = new StringBuilder();
                    foreach (XElement itemElement in linkedElements.Last.Value.Elements()) {
                        sb.Append($"\"{itemElement.Value}\",");
                    }
                    if (sb.Length > 0)
                        sb.Remove(sb.Length - 1, 1);
                    this.Add(LinkedToKey(linkedElements), sb.ToString());
                }
                break;
            case TagCategory.FlagItem:
            case TagCategory.ListSimpleItem:
            case TagCategory.ListDefItem:
            case TagCategory.ListRefItem:
            case TagCategory.DictItem: {
                    Log.Warning();
                    Log.WriteLine(@"Entered invalid tag accidentally: {LinkedToKey(linkedElements)}");
                }
                break;
            case TagCategory.ListRef: {
                    StringBuilder sb = new StringBuilder();
                    foreach (XElement itemElement in linkedElements.Last.Value.Elements()) {
                        sb.Append($"\"{itemElement.Name.ToString()}\",\"{itemElement.Value}\";");
                    }
                    if (sb.Length > 0)
                        sb.Remove(sb.Length - 1, 1);
                    this.Add(LinkedToKey(linkedElements), sb.ToString());
                }
                break;
            case TagCategory.Dict: {
                    StringBuilder sb = new StringBuilder();
                    foreach (XElement itemElement in linkedElements.Last.Value.Elements()) {
                        sb.Append($"\"{itemElement.Element("key").Value}\",\"{itemElement.Element("value").Value}\";");
                    }
                    if (sb.Length > 0)
                        sb.Remove(sb.Length - 1, 1);
                    this.Add(LinkedToKey(linkedElements), sb.ToString());
                }
                break;
            default:
                break;
            }
        }


        private static string LinkedToKey(LinkedList<XElement> linkedElements, bool isCount = false) {
            StringBuilder sb = new StringBuilder();
            foreach (XElement element in linkedElements) {
                string name = element.Name.ToString();
                if (name == "li") {
                    sb.Append(element.Attribute("ListIndex").Value);
                } else {
                    sb.Append(name);
                }
                //string className = element.GetClassName();
                //if (className != null)
                //{
                //    sb.Append($"({className})");
                //}
                sb.Append('.');
            }
            XAttribute lang = linkedElements.Last.Value.Attribute("lang");
            if (lang == null) {
                sb.Remove(sb.Length - 1, 1);
            } else {
                sb.Append(lang.Value);
            }
            if (isCount) {
                sb.Append(".Count");
            }
            return sb.ToString();
        }

        #endregion

        #region Helper

        public bool CanBeRange(LinkedList<XElement> linkedElements, TagInfo tagInfo) {
            XElement tagElement = linkedElements.Last.Value;
            if (tagInfo.HasTags && tagInfo.Tags.Count() == 2 && tagElement.Elements().Count() == 2) {
                XElement minElement = tagElement.Element("min");
                XElement maxElement = tagElement.Element("max");
                if (minElement != null && maxElement != null) {
                    this.Add(LinkedToKey(linkedElements), $"{minElement.Value} ~ {maxElement.Value}");
                    return true;
                }
            }
            return false;
        }

        public bool CanBeListRef(LinkedList<XElement> linkedElements, TagInfo tagInfo) {
            XElement tagElement = linkedElements.Last.Value;
            if (tagInfo.HasTags && tagInfo.Tags.Count() == 1) {
                TagInfo item = tagInfo.Tags.First();
                if (item.HasTags && item.Tags.Count() == 2) {
                    TagInfo def = null;
                    foreach (TagInfo curSubTagInfo in item.Tags) {
                        if (curSubTagInfo.TagType.IsSubclassOf(Helper.ClassDef)) {
                            def = curSubTagInfo;
                            break;
                        }
                    }
                    TagInfo value = null;
                    foreach (TagInfo curSubTagInfo in item.Tags) {
                        if (!curSubTagInfo.TagType.IsSubclassOf(Helper.ClassDef)) {
                            value = curSubTagInfo;
                            break;
                        }
                    }
                    if (def != null && value != null) {
                        foreach (XElement curItemElement in tagElement.Elements()) {
                            XElement defElement = curItemElement.Element(def.Name);
                            XElement valueElemnt = curItemElement.Element(value.Name);
                            if (defElement == null || value == null) {
                                return false;
                            }
                        }
                        StringBuilder sb = new StringBuilder();
                        foreach (XElement curItemElement in tagElement.Elements()) {
                            XElement defElement = curItemElement.Element(def.Name);
                            XElement valueElemnt = curItemElement.Element(value.Name);
                            sb.Append($"\"{defElement.Value}\",\"{valueElemnt.Value}\";");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        this.Add(LinkedToKey(linkedElements), sb.ToString());
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CanBeListRefRange(LinkedList<XElement> linkedElements, TagInfo tagInfo) {
            XElement tagElement = linkedElements.Last.Value;
            if (tagInfo.HasTags && tagInfo.Tags.Count() == 1) {
                TagInfo item = tagInfo.Tags.First();
                if (item.HasTags && item.Tags.Count() == 3) {
                    TagInfo def = null;
                    foreach (TagInfo curSubTagInfo in item.Tags) {
                        if (curSubTagInfo.TagType.IsSubclassOf(Helper.ClassDef)) {
                            def = curSubTagInfo;
                            break;
                        }
                    }
                    TagInfo min = null;
                    foreach (TagInfo curSubTagInfo in item.Tags) {
                        if (curSubTagInfo.Name == "min") {
                            min = curSubTagInfo;
                            break;
                        }
                    }
                    TagInfo max = null;
                    foreach (TagInfo curSubTagInfo in item.Tags) {
                        if (curSubTagInfo.Name == "max") {
                            max = curSubTagInfo;
                            break;
                        }
                    }
                    if (def != null && min != null && max != null) {
                        foreach (XElement curItemElement in tagElement.Elements()) {
                            XElement defElement = curItemElement.Element(def.Name);
                            XElement minElemnet = curItemElement.Element("min");
                            XElement maxElement = curItemElement.Element("max");
                            if (defElement == null || minElemnet == null || maxElement == null) {
                                return false;
                            }
                        }
                        StringBuilder sb = new StringBuilder();
                        foreach (XElement curItemElement in tagElement.Elements()) {
                            XElement defElement = curItemElement.Element(def.Name);
                            XElement minElemnet = curItemElement.Element("min");
                            XElement maxElement = curItemElement.Element("max");
                            sb.Append($"\"{defElement.Value}\",\"{minElemnet.Value} ~ {maxElement.Value}\";");
                        }
                        sb.Remove(sb.Length - 1, 1);
                        this.Add(LinkedToKey(linkedElements), sb.ToString());
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region ToString and Save

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{{#set:");
            sb.AppendLine("|dataType=Def");
            sb.Append("|defType=");
            sb.AppendLine(this.defType);
            sb.Append("|defName=");
            sb.AppendLine(this.defName);
            foreach (var kvp in this.dict) {
                sb.Append('|');
                sb.Append(kvp.Key);
                sb.Append('=');
                sb.AppendLine(kvp.Value);
            }
            sb.Append("}}");
            return sb.ToString();
        }


        /// <summary>
        /// Save this set dict to a file.
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path) {
            File.WriteAllText(path, this.ToString(), utf8);
        }

        #endregion
    }
}
