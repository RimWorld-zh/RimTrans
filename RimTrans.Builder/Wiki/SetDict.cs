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

namespace RimTrans.Builder.Wiki
{
    public class SetDict
    {
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
        public SetDict(XElement defElement, DefInfo defInfo)
        {
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

            foreach (TagInfo curTagInfo in defInfo.Tags)
            {
                string curName = curTagInfo.Name;
                string curClassName = curTagInfo.ClassName;
                bool isMatched = false;
                foreach (XElement curTagElement in defElement.Elements())
                {
                    if (curTagElement.Name.ToString() == curName &&
                        curTagElement.GetClassName() == curClassName)
                    {
                        isMatched = true;
                        linkedElements.AddLast(curTagElement);
                        this.AddMatched(linkedElements, curTagInfo);
                        linkedElements.RemoveLast();
                    }
                }
                if (!isMatched)
                {
                    this.AddNonMatched(curTagInfo);
                }
            }
        }

        private void AddNonMatched(TagInfo tagInfo)
        {

        }

        private void AddMatched(LinkedList<XElement> linkedElements, TagInfo tagInfo)
        {
            switch (tagInfo.Category)
            {
                case TagCategory.Unkown:
                    Log.Error();
                    Log.WriteLine($"Unkown tag '{linkedElements.Last.Value.Name.ToString()}'");
                    break;
                case TagCategory.Standard:
                case TagCategory.ListComplexItem:
                    {
                        XElement tagElement = linkedElements.Last.Value;
                        if (tagElement.HasElements)
                        {
                            if (tagInfo.HasTags)
                            {
                                foreach (TagInfo curSubTagInfo in tagInfo.Tags)
                                {
                                    string curSubName = curSubTagInfo.Name;
                                    string curSubClassName = curSubTagInfo.ClassName;
                                    bool isMatched = false;
                                    foreach (XElement curSubTagElement in tagElement.Elements())
                                    {
                                        if (curSubTagElement.Name.ToString() == curSubName &&
                                            curSubTagElement.GetClassName() == curSubClassName)
                                        {
                                            isMatched = true;
                                            linkedElements.AddLast(curSubTagElement);
                                            this.AddMatched(linkedElements, curSubTagInfo);
                                            linkedElements.RemoveLast();
                                        }
                                    }
                                    if (!isMatched)
                                    {
                                        this.AddNonMatched(curSubTagInfo);
                                    }
                                }
                            }
                            else
                            {
                                Log.Warning();
                                Log.WriteLine($"Tag '{LinkedToKey(linkedElements)}' has sub-elements, but matched TagInfo that has no sub-tags.");
                            }
                        }
                        else
                        {
                            this.Add(LinkedToKey(linkedElements), linkedElements.Last.Value.Value);
                        }
                    }
                    break;
                case TagCategory.ListComplex:
                    {
                        XElement tagElement = linkedElements.Last.Value;
                        if (tagElement.HasElements)
                        {
                            if (tagInfo.HasTags)
                            {
                                foreach (XElement curItemElement in tagElement.Elements())
                                {
                                    foreach (TagInfo curSubTagInfo in tagInfo.Tags)
                                    {
                                        if (curItemElement.GetClassName() == curSubTagInfo.ClassName)
                                        {
                                            linkedElements.AddLast(curItemElement);
                                            this.AddMatched(linkedElements, curSubTagInfo);
                                            linkedElements.RemoveLast();
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log.Warning();
                                Log.WriteLine($"Tag '{LinkedToKey(linkedElements)}' has sub-elements, but matched TagInfo that has no sub-tags.");
                            }
                        }
                    }
                    break;
                case TagCategory.Def:
                case TagCategory.Enum:
                    {
                        this.Add(LinkedToKey(linkedElements), linkedElements.Last.Value.Value);
                    }
                    break;
                case TagCategory.Flag:
                case TagCategory.ListSimple:
                case TagCategory.ListDef:
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (XElement itemElement in linkedElements.Last.Value.Elements())
                        {
                            sb.Append($"\"{itemElement.Value}\",");
                        }
                        if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
                        this.Add(LinkedToKey(linkedElements), sb.ToString());
                    }
                    break;
                case TagCategory.FlagItem:
                case TagCategory.ListSimpleItem:
                case TagCategory.ListDefItem:
                case TagCategory.ListRefItem:
                case TagCategory.DictItem:
                    {
                        Log.Warning();
                        Log.WriteLine(@"Entered invalid tag accidentally: {LinkedToKey(linkedElements)}");
                    }
                    break;
                case TagCategory.ListRef:
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (XElement itemElement in linkedElements.Last.Value.Elements())
                        {
                            sb.Append($"\"{itemElement.Name.ToString()}\",\"{itemElement.Value}\";");
                        }
                        if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
                        this.Add(LinkedToKey(linkedElements), sb.ToString());
                    }
                    break;
                case TagCategory.Dict:
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (XElement itemElement in linkedElements.Last.Value.Elements())
                        {
                            sb.Append($"\"{itemElement.Element("key").Value}\",\"{itemElement.Element("value").Value}\";");
                        }
                        if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
                        this.Add(LinkedToKey(linkedElements), sb.ToString());
                    }
                    break;
                default:
                    break;
            }
        }


        private static string LinkedToKey(LinkedList<XElement> linkedElements)
        {
            StringBuilder sb = new StringBuilder();
            foreach (XElement element in linkedElements)
            {
                string name = element.Name.ToString();
                if (name == "li")
                {
                    sb.Append(element.Attribute("ListIndex").Value);
                }
                else
                {
                    sb.Append(name);
                }
                string className = element.GetClassName();
                if (className != null)
                {
                    sb.Append($"({className})");
                }
                sb.Append('.');
            }
            XAttribute lang = linkedElements.Last.Value.Attribute("lang");
            if (lang == null)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            else
            {
                sb.Append(lang.Value);
            }
            return sb.ToString();
        }

        public void Add(string key, string value)
        {
            if (this.dict.ContainsKey(key))
            {
                Log.Warning();
                Log.WriteLine($"Duplicated key: key='{key}', value='{value}'.");
            }
            else
            {
                this.dict.Add(key, value);
            }
        }

        #endregion

        #region ToString and Save

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{{#set:");
            sb.AppendLine("|dataType=Def");
            sb.AppendLine($"|defType={this.defType}");
            sb.AppendLine($"|defName={this.defName}");
            foreach (var kvp in this.dict)
            {
                sb.AppendLine($"|{kvp.Key}={kvp.Value}");
            }
            sb.Append("}}");
            return sb.ToString();
        }


        /// <summary>
        /// Save this set dict to a file.
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("{{#set:");
                sw.WriteLine("|dataType=Def");
                sw.WriteLine($"|defType={this.defType}");
                sw.WriteLine($"|defName={this.defName}");
                foreach (var kvp in this.dict)
                {
                    sw.WriteLine($"|{kvp.Key}={kvp.Value}");
                }
                sw.Write("}}");
            }
        }

        #endregion
    }
}
