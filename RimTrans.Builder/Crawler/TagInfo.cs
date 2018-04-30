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

using RimTrans.Builder.Xml;

namespace RimTrans.Builder.Crawler {
    /// <summary>
    /// Storage Tag info.
    /// </summary>
    public class TagInfo {
        #region Fields

        /// <summary>
        /// The Def that declared this tag.
        /// </summary>
        public DefInfo DeclareDef { get { return this.declareDef; } }
        private DefInfo declareDef;


        /// <summary>
        /// The parent tag of this tag.
        /// </summary>
        public TagInfo Parent { get { return this.parent; } }
        private TagInfo parent;


        /// <summary>
        /// Which category this tag is. 
        /// </summary>
        public TagCategory Category { get { return this.tagCategory; } }
        private TagCategory tagCategory;


        /// <summary>
        /// Get name of this tag.
        /// </summary>
        public string Name { get { return this.tagName; } }
        private readonly string tagName;


        /// <summary>
        /// Get type of this tag.
        /// </summary>
        public Type TagType { get { return this.tagType; } }
        private Type tagType;


        /// <summary>
        /// Get class name of this tag.
        /// </summary>
        public string ClassName { get { return this.className; } }
        private string className;


        /// <summary>
        /// Get type name of this tag.
        /// </summary>
        public string TypeName { get { return this.typeName; } }
        private string typeName;


        /// <summary>
        /// Get availabel values while the type is enum.
        /// </summary>
        public string AvailableValues { get { return this.availableValues; } }
        private string availableValues;


        /// <summary>
        /// Get default value as string of this tag.
        /// </summary>
        public string DefaultValueAsString { get { return this.defaultValueAsString; } }
        private string defaultValueAsString;


        /// <summary>
        /// Get description of this tag.
        /// </summary>
        public string Description { get { return this.description; } }
        private string description;


        /// <summary>
        /// Get alias of this tag.
        /// </summary>
        public string Alias { get { return this.alias; } }
        private string alias;


        /// <summary>
        /// Get all sub-fields of this Tag.
        /// </summary>
        public IEnumerable<FieldInfo> Fields { get { return this.allFields.Values; } }
        private Dictionary<string, FieldInfo> allFields;


        /// <summary>
        /// Get all sub-Tags of this Tag.
        /// </summary>
        public IEnumerable<TagInfo> Tags { get { return this.allTags.Values; } }
        private Dictionary<string, TagInfo> allTags;

        /// <summary>
        /// If this tag has sub tags.
        /// </summary>
        public bool HasTags { get { return this.allTags != null && this.allTags.Count > 0; } }


        /// <summary>
        /// Non-matched elements.
        /// </summary>
        public IEnumerable<XElement> NonMatchedElements { get { return this.nonMatchedElements; } }
        private List<XElement> nonMatchedElements;

        /// <summary>
        /// If this tag has non-matched elements.
        /// </summary>
        public bool HasNonMatched { get { return this.nonMatchedElements != null && this.nonMatchedElements.Count > 0; } }


        /// <summary>
        /// Get pseudo value text.
        /// </summary>
        public string PseudoValue {
            get {
                if (this.tagType != null) {
                    if (this.tagType == typeof(string))
                        return "string_value";
                    if (this.tagType == typeof(int))
                        return "int_value";
                    if (this.tagType == typeof(float))
                        return "float_value";
                    if (this.tagType == typeof(double))
                        return "double_value";
                    if (this.tagType == typeof(bool))
                        return "true_or_false";
                    if (this.tagType.IsEnum)
                        return "enum_value"; //$"{this.tagType.Name}_value";
                    if (this.tagType == typeof(Type))
                        return "class_name";
                    if (this.tagType.IsSubclassOf(Helper.ClassDef))
                        return $"{this.tagType.Name}_defName";
                }
                return "value";
            }
        }

        #endregion

        #region Init


        /// <summary>
        /// Initialize a TagInfo from xml element, FieldInfo of this tag and instanced parent object.
        /// </summary>
        /// <param name="declare">The Def that declared this tag.</param>
        /// <param name="tagElement">The xml element of this tag.</param>
        /// <param name="fieldInfo">The FieldInfo of this tag.</param>
        /// <param name="parent">The parent class which the field belong to.</param>
        /// <param name="tagClass">If class name of the tag is not null, use this.</param>
        public TagInfo(DefInfo declare, XElement tagElement, FieldInfo fieldInfo, Type tagClass = null)
            : this((TagInfo)null, tagElement, fieldInfo, tagClass) {
            this.declareDef = declare;
        }


        /// <summary>
        /// Initialize a TagInfo from xml element, FieldInfo of this tag and instanced parent object.
        /// </summary>
        /// <param name="parent">The parent tag of this tag.</param>
        /// <param name="tagElement">The xml element of this tag.</param>
        /// <param name="fieldInfo">The FieldInfo of this tag.</param>
        /// <param name="parent">The parent class which the field belong to.</param>
        /// <param name="tagClass">If class name of the tag is not null, use this.</param>
        private TagInfo(TagInfo parent, XElement tagElement, FieldInfo fieldInfo, Type tagClass = null) {
            if (tagElement.GetClassName() != null && tagClass == null)
                throw new ArgumentNullException("tagClass", "The argument 'tagClass' can not be null, because of 'tagElement' has class name.");

            if (tagElement.GetClassName() == null && tagClass != null)
                throw new ArgumentException("The argument 'tagElement' has no class name, but 'tagClass' is not null.", "tagClass");

            if (string.Compare(tagElement.Name.ToString(), fieldInfo.Name, true) != 0)
                throw new ArgumentException("The names of arguments 'tagElement' and 'fieldInfo' no matched.");

            if (tagElement.Name.ToString() == "li")
                throw new ArgumentException("Can not construct tag from list item.", "tagElement");

            XElement tag = new XElement(tagElement);

            this.tagName = fieldInfo.Name;
            this.className = tag.GetClassName();
            if (this.className == null) {
                this.tagType = fieldInfo.FieldType;
                this.typeName = Helper.GetTypeName(this.tagType);
                this.availableValues = Helper.GetAvailableValues(this.tagType);
                this.defaultValueAsString = Helper.GetDefaultValue(fieldInfo);
            } else {
                this.tagType = tagClass;
                this.typeName = Helper.GetTypeName(this.tagType);
                this.availableValues = null;
                this.defaultValueAsString = null;
            }
            this.description = Helper.GetDescription(fieldInfo);
            this.alias = Helper.GetAlias(fieldInfo);

            if (tag.HasElements) {
                if (this.tagType.IsGenericType) {
                    this.CreateChildrenGeneric(tag);
                } else if (this.tagType.IsEnum) {
                    this.tagCategory = TagCategory.Flag;
                    this.CreateChildrenFlag(tag);
                } else {
                    this.tagCategory = TagCategory.Standard;
                    this.CreateChildren(tag);
                }
            } else {
                if (this.tagType.IsSubclassOf(Helper.ClassDef)) {
                    this.tagCategory = TagCategory.Def;
                } else if (this.tagType.IsEnum) {
                    this.tagCategory = TagCategory.Enum;
                } else {
                    this.tagCategory = TagCategory.Standard;
                }
            }
        }


        /// <summary>
        /// Initialize a list item TagInfo.
        /// </summary>
        /// <param name="itemElement">The list item xml element.</param>
        /// <param name="itemType">List item type.</param>
        /// <param name="itemClass">If the list item has class name, use this.</param>
        private TagInfo(TagInfo parent, XElement itemElement, Type itemType, Type itemClass = null) {
            if (itemElement.GetClassName() != null && itemClass == null)
                throw new ArgumentNullException("itemClass", "The argument 'itemClass' can not be null, because of 'itemElement' has class name.");

            if (itemElement.GetClassName() == null && itemClass != null)
                throw new ArgumentException("The argument 'itemElement' has no class name, but 'itemClass' is not null.", "itemClass");

            if (itemElement.Name.ToString() != "li" && itemClass != null)
                throw new ArgumentException("Invalid element: 'itemElement' not named 'li' and itemClass not null.");

            XElement item = new XElement(itemElement);

            this.parent = parent;
            this.tagName = "li";
            this.className = itemElement.GetClassName();
            if (this.className == null) {
                this.tagType = itemType;
                this.typeName = Helper.GetTypeName(this.tagType);
                this.availableValues = Helper.GetAvailableValues(this.tagType);
                this.defaultValueAsString = null;
            } else {
                this.tagType = itemClass;
                this.typeName = Helper.GetTypeName(this.tagType);
                this.availableValues = null;
                this.defaultValueAsString = null;
            }
            this.description = null;
            this.alias = null;

            // List
            if (item.Name.ToString() == "li") {
                // List Complex
                if (item.HasElements) {
                    this.tagCategory = TagCategory.ListComplexItem;
                    this.CreateChildren(item);
                }
                // List Simple
                else {
                    if (this.tagType.IsSubclassOf(Helper.ClassDef)) {
                        this.tagCategory = TagCategory.ListDefItem;
                    } else if (this.className != null && item.Value == string.Empty) {
                        this.tagCategory = TagCategory.ListComplexItem;
                    } else {
                        this.tagCategory = TagCategory.ListSimpleItem;
                    }
                }
            }
            // Dict Item Key
            else if (item.Name.ToString() == "key") {
                if (item.HasElements) {
                    Log.Error();
                    Log.WriteLine("Tag 'key' has sub-elements.");
                } else {
                    this.tagName = "key";
                    if (this.tagType.IsSubclassOf(Helper.ClassDef)) {
                        this.tagCategory = TagCategory.Def;
                    } else {
                        this.tagCategory = TagCategory.Standard;
                    }
                }
            }
            // Dict Item Value
            else if (item.Name.ToString() == "value") {
                if (item.HasElements) {
                    Log.Error();
                    Log.WriteLine("Tag 'value' has sub-elements.");
                } else {
                    this.tagName = "value";
                    if (this.tagType.IsSubclassOf(Helper.ClassDef)) {
                        this.tagCategory = TagCategory.Def;
                    } else {
                        this.tagCategory = TagCategory.Standard;
                    }
                }
            }
            // List Ref Item
            else {
                this.tagCategory = TagCategory.ListRefItem;
                this.tagName = null;
                FieldInfo[] fieldInfoArray = this.tagType.GetFields();
                if (fieldInfoArray.Count() == 2) {
                    this.allTags = new Dictionary<string, TagInfo>();
                    TagInfo refKey = new TagInfo(this, fieldInfoArray[0]);
                    this.allTags.Add("refKey", refKey);
                    TagInfo refValue = new TagInfo(this, fieldInfoArray[1]);
                    this.allTags.Add("refValue", refValue);
                } else {
                    Log.Error();
                    Log.WriteLine($"The fields amount of the ref item '{item.Name.ToString()}' is not 2.");
                }
            }
        }


        /// <summary>
        /// Initialize a Reference Item TagInfo
        /// </summary>
        /// <param name="tagName"></param>
        private TagInfo(TagInfo parent, FieldInfo fieldInfo) {
            this.parent = parent;
            this.tagCategory = TagCategory.ListRefItem;
            this.tagName = fieldInfo.Name;
            this.tagType = fieldInfo.FieldType;
            this.typeName = Helper.GetTypeName(this.tagType);
        }

        /// <summary>
        /// Initialize a dictionary item (KeyValuePair) TagInfo.
        /// </summary>
        /// <param name="keyType"></param>
        /// <param name="valueType"></param>
        public TagInfo(TagInfo parent, XElement keyElement, Type keyType, XElement valueElement, Type valueType) {
            if (keyElement.Name.ToString() != "key" || keyElement.HasElements)
                throw new ArgumentException("Invalid element.", "keyElement");

            if (valueElement.Name.ToString() != "value" || valueElement.HasElements)
                throw new ArgumentException("Invalid element.", "valueElement");

            this.parent = parent;
            this.tagCategory = TagCategory.DictItem;
            this.tagName = "li";
            this.className = null;
            this.tagType = null;
            this.typeName = null;
            this.availableValues = null;
            this.defaultValueAsString = null;
            this.description = null;
            this.alias = null;

            this.allTags = new Dictionary<string, TagInfo>();

            allTags.Add("key", new TagInfo(this, keyElement, keyType));
            allTags.Add("value", new TagInfo(this, valueElement, valueType));
        }


        /// <summary>
        /// Initialize a flag item TagInfo from enum type.
        /// </summary>
        /// <param name="flagEnumType"></param>
        private TagInfo(TagInfo parent, Type flagEnumType) {
            if (!flagEnumType.IsEnum)
                throw new ArgumentException("Invalid type, must use enum type.", "flagEnumType");

            this.parent = parent;
            this.tagCategory = TagCategory.FlagItem;
            this.tagName = "li";
            this.className = null;
            this.tagType = flagEnumType;
            this.typeName = null;
            this.availableValues = null;
            this.defaultValueAsString = null;
            this.description = null;
            this.alias = null;
        }

        #endregion

        #region Secondary Methods

        /// <summary>
        /// Create sub tags
        /// </summary>
        /// <param name="tag"></param>
        private void CreateChildren(XElement tag) {
            this.allFields = new Dictionary<string, FieldInfo>();
            this.allTags = new Dictionary<string, TagInfo>();
            this.nonMatchedElements = new List<XElement>();

            LinkedList<Type> types = new LinkedList<Type>();
            {
                types.AddFirst(this.tagType);
                Type curType = this.tagType;
                while (curType != Helper.ClassObject) {
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
                foreach (XElement curAliasElement in tag.Elements()) {
                    if (string.Compare(curAliasElement.Name.ToString(), curAlias, true) == 0) {
                        bool isMatched = false;
                        foreach (XElement curElement in tag.Elements()) {
                            if (string.Compare(curElement.Name.ToString(), curName, true) == 0 &&
                                curAliasElement.GetClassName() == curElement.GetClassName()) {
                                isMatched = true;
                                Helper.MergeElement(curElement, curAliasElement);
                                Helper.MergeListItems(curElement);
                                aliasElements.Add(curAliasElement);
                                break;
                            }
                        }
                        if (!isMatched) {
                            curAliasElement.Name = curName;
                        }
                    }
                }
                foreach (XElement curAliasElement in aliasElements) {
                    curAliasElement.Remove();
                }

                foreach (XElement curElement in tag.Elements()) {
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

            foreach (XElement curElement in tag.Elements()) {
                if (!matchedElements.Contains(curElement)) {
                    Log.Warning();
                    Log.WriteLine($"The element '{curElement.Name.ToString()}' of {this.tagName} no matched.");
                    this.nonMatchedElements.Add(curElement);
                }
            }
        }


        /// <summary>
        /// Create sub tags (flag enum)
        /// </summary>
        /// <param name="tag"></param>
        private void CreateChildrenFlag(XElement tag) {
            this.allTags = new Dictionary<string, TagInfo>();
            this.allTags.Add("li", new TagInfo(this, this.tagType));
        }


        /// <summary>
        /// Create sub tags (generic type)
        /// </summary>
        /// <param name="tag"></param>
        private void CreateChildrenGeneric(XElement tag) {
            this.allTags = new Dictionary<string, TagInfo>();
            this.nonMatchedElements = new List<XElement>();

            string generticTypeName = this.typeName.Substring(0, this.typeName.IndexOf('<'));
            Type[] genericArgumentsArray = this.tagType.GetGenericArguments();
            if (genericArgumentsArray.Count() == 1) {
                if (generticTypeName == "List") {
                    int elementCount = tag.Elements().Count();
                    int liCount = tag.Elements("li").Count();
                    Type itemType = genericArgumentsArray[0];
                    // List
                    if (liCount > 0 && liCount == elementCount) {
                        if (itemType.IsSubclassOf(Helper.ClassDef)) {
                            this.tagCategory = TagCategory.ListDef;
                        } else {
                            this.tagCategory = TagCategory.ListSimple;
                            foreach (XElement curElement in tag.Elements())
                                if (curElement.HasElements) {
                                    this.tagCategory = TagCategory.ListComplex;
                                    break;
                                }
                        }
                        IEnumerable<Type> allSubClasses = itemType.AllSubclasses();
                        List<XElement> matchedElements = new List<XElement>();
                        foreach (XElement curElement in tag.Elements()) {
                            string className = curElement.GetClassName();
                            if (className == null) {
                                string key = "li";
                                if (!this.allTags.ContainsKey(key)) {
                                    this.allTags.Add(key, new TagInfo(this, curElement, itemType));
                                    matchedElements.Add(curElement);
                                }
                            } else {
                                foreach (Type curSubClass in allSubClasses) {
                                    if (className == curSubClass.Name) {
                                        string key = $"li {className}";
                                        if (!this.allTags.ContainsKey(key)) {
                                            this.allTags.Add(key, new TagInfo(this, curElement, itemType, curSubClass));
                                            matchedElements.Add(curElement);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        foreach (XElement curElement in tag.Elements()) {
                            if (!matchedElements.Contains(curElement)) {
                                Log.Error();
                                Log.WriteLine($"The element '{curElement.Name.ToString()}' of {this.tagName} no matched.");
                                this.nonMatchedElements.Add(curElement);
                            }
                        }
                    }
                    // List Reference
                    else if (liCount == 0 && elementCount > 0) {
                        this.tagCategory = TagCategory.ListRef;
                        this.allTags.Add("li", new TagInfo(this, tag.Elements().First(), itemType));
                    }
                    // Unkown
                    else {
                        Log.Error();
                        Log.WriteLine($"Tag '{this.tagName}' has both type of itme.");
                    }
                } else {
                    Log.Error();
                    Log.WriteLine($"Tag '{this.tagName}' is unkown generic type: {this.typeName}.");
                }
            } else if (genericArgumentsArray.Count() == 2) {
                if (generticTypeName == "Dictionary") {
                    int elementCount = tag.Elements().Count();
                    int liCount = tag.Elements().Count();
                    if (liCount == 1 && liCount == elementCount) {
                        this.tagCategory = TagCategory.Dict;
                        XElement itemElement = tag.Element("li");
                        XElement keyElement = itemElement.Element("key");
                        Type keyType = genericArgumentsArray[0];
                        XElement valueElement = itemElement.Element("value");
                        Type valueType = genericArgumentsArray[1];
                        this.allTags.Add("li", new TagInfo(this, keyElement, keyType, valueElement, valueType));
                    } else {
                        Log.Error();
                        Log.WriteLine($"Tag '{this.tagName}' is unkown generic type: {this.typeName}.");
                    }

                } else {
                    Log.Error();
                    Log.WriteLine($"Tag '{this.tagName}' is unkown generic type: {this.typeName}.");
                }
            }
        }

        #endregion


        #region ToXNodes

        public IEnumerable<XNode> ToXNodes() {
            switch (this.tagCategory) {
            case TagCategory.Unkown:
                yield return new XComment("======== error ========");
                break;
            case TagCategory.Standard:
            case TagCategory.Flag:
            case TagCategory.ListSimple:
            case TagCategory.ListComplex:
            case TagCategory.ListComplexItem:
            case TagCategory.ListDef:
            case TagCategory.ListRef:
            case TagCategory.Dict:
                if (this.tagCategory == TagCategory.ListSimple)
                    yield return new XComment($"Simple List");
                if (this.tagCategory == TagCategory.ListComplex)
                    yield return new XComment($"Complex List");
                if (this.tagCategory == TagCategory.ListDef)
                    yield return new XComment($"Reference List");
                if (this.tagCategory == TagCategory.Flag)
                    yield return new XComment($"Flag");
                if (this.typeName != null)
                    yield return new XComment($"Type: {this.typeName}");
                if (this.availableValues != null)
                    yield return new XComment($"Available Values: {this.availableValues}");
                if (this.defaultValueAsString != null) {
                    switch (this.tagCategory) {
                    case TagCategory.Unkown:
                    case TagCategory.Standard:
                        yield return new XComment($"Default Value: {this.defaultValueAsString}");
                        break;
                    }
                }
                if (this.description != null)
                    yield return new XComment($"Desc: {this.description}");
                if (this.Alias != null)
                    yield return new XComment($"Alias: {this.alias}");
                XElement tag = new XElement(this.tagName);
                if (this.className != null)
                    tag.SetAttributeValue("Class", this.className);
                if (this.HasTags) {
                    foreach (TagInfo subTag in this.allTags.Values)
                        tag.Add(subTag.ToXNodes());
                    switch (this.tagCategory) {
                    case TagCategory.Flag:
                    case TagCategory.ListSimple:
                    case TagCategory.ListComplex:
                    case TagCategory.ListDef:
                    case TagCategory.ListRef:
                    case TagCategory.Dict:
                        tag.Add(new XComment(" ... "));
                        break;
                    }
                }
                if (this.HasNonMatched) {
                    tag.Add(new XComment("======== Non Matched Elements ========"));
                    tag.Add(this.nonMatchedElements);
                }
                if (tag.Nodes().Count() == 0 && this.tagCategory != TagCategory.ListComplexItem)
                    tag.Value = this.PseudoValue;
                yield return tag;
                break;
            case TagCategory.Def:
            case TagCategory.Enum:
                if (this.typeName != null)
                    yield return new XComment($"Type: {this.typeName}");
                if (this.availableValues != null)
                    yield return new XComment($"Available Values: {this.availableValues}");
                if (this.defaultValueAsString != null)
                    yield return new XComment($"Default Value: {this.defaultValueAsString}");
                if (this.description != null)
                    yield return new XComment($"Desc: {this.description}");
                if (this.Alias != null)
                    yield return new XComment($"Alias: {this.alias}");
                yield return new XElement(this.tagName, this.PseudoValue);
                break;
            case TagCategory.FlagItem:
            case TagCategory.ListSimpleItem:
            case TagCategory.ListDefItem:
                if (this.typeName != null)
                    yield return new XComment($"Type: {this.typeName}");
                if (this.availableValues != null)
                    yield return new XComment($"Available Values: {this.availableValues}");
                yield return new XElement(this.tagName, this.PseudoValue);
                break;
            case TagCategory.ListRefItem:
                if (this.typeName != null)
                    yield return new XComment($"Type: {this.typeName}");
                if (this.HasTags) {
                    TagInfo refKey = this.allTags["refKey"];
                    TagInfo refValue = this.allTags["refValue"];
                    yield return new XComment($"Tag Field: {refKey.tagName}, Tag Type: {refKey.typeName}");
                    yield return new XComment($"Value Field: {refValue.tagName}, Value Type: {refValue.typeName}");
                    yield return new XComment($"Pattern: <{refKey.PseudoValue}>{refValue.PseudoValue}</{refKey.PseudoValue}>");
                }
                break;
            case TagCategory.DictItem:
                XElement dictItem = new XElement(this.tagName);
                if (this.HasTags) {
                    foreach (TagInfo subTag in this.allTags.Values)
                        dictItem.Add(subTag.ToXNodes());
                }
                yield return dictItem;
                break;
            default:
                break;
            }
        }

        #endregion


        #region ProcessFieldNames

        /// <summary>
        /// Process case and alias of all fields in this definition data.
        /// </summary>
        /// <param name="tag"></param>
        public void ProcessFieldNames(XElement tag) {
            string temp = tag.Name.ToString();
            if ((string.Compare(temp, this.tagName, true) != 0 && string.Compare(temp, this.alias, true) != 0) ||
                tag.GetClassName() != this.className)
                throw new ArgumentException($"Tag '{tag.Name.ToString()}' and TagInfo '{this.tagName}': name or class name no matched.", "tag");

            switch (this.tagCategory) {
            case TagCategory.Unkown:
                Log.Warning();
                Log.WriteLine($"Tag '{tag.Name.ToString()}' matched unkown TagInfo.");
                return;
            case TagCategory.Standard:
            case TagCategory.Def:
            case TagCategory.Enum:
            case TagCategory.Flag:
            case TagCategory.ListSimple:
            case TagCategory.ListComplex:
            case TagCategory.ListDef:
            case TagCategory.ListRef:
            case TagCategory.Dict:
                tag.Name = this.tagName;
                break;
            case TagCategory.FlagItem:
            case TagCategory.ListSimpleItem:
            case TagCategory.ListComplexItem:
            case TagCategory.ListDefItem:
            case TagCategory.ListRefItem:
            case TagCategory.DictItem:
            default:
                break;
            }

            if (!tag.HasElements)
                return;

            switch (this.tagCategory) {
            case TagCategory.Unkown:
                Log.Warning();
                Log.WriteLine($"Tag '{tag.Name.ToString()}' matched unkown TagInfo.");
                return;
            case TagCategory.Standard:
            case TagCategory.ListComplexItem: {
                    if (!this.HasTags) {
                        Log.Warning();
                        Log.WriteLine($"Tag '{tag.Name.ToString()}' has sub-elements, but matched TagInfo that has no sub-tags.");
                        return;
                    }

                    var allSubTagInfos = this.allTags.Values;
                    List<XElement> matchedSubTags = new List<XElement>();
                    Dictionary<XElement, XComment> bufferComments = new Dictionary<XElement, XComment>();
                    foreach (TagInfo curSubTagInfo in allSubTagInfos) {
                        string curSubName = curSubTagInfo.Name;
                        string curSubAlias = curSubTagInfo.Alias;
                        bool flag = true;
                        while (flag) {
                            flag = false;
                            foreach (XElement curSubTag in tag.Elements()) {
                                string curSubTagName = curSubTag.Name.ToString();
                                if ((string.Compare(curSubTagName, curSubName, true) == 0 ||
                                    string.Compare(curSubTagName, curSubAlias, true) == 0) &&
                                    curSubTag.GetClassName() == curSubTagInfo.ClassName) {
                                    flag = true;
                                    XNode node = curSubTag.PreviousNode;
                                    if (node != null && node.NodeType == XmlNodeType.Comment) {
                                        node.Remove();
                                        bufferComments.Add(curSubTag, node as XComment);
                                    }
                                    curSubTagInfo.ProcessFieldNames(curSubTag);
                                    curSubTag.Remove();
                                    matchedSubTags.Add(curSubTag);
                                    break;
                                }
                            }
                        }
                    }
                    List<XElement> nonMatchedElements = new List<XElement>();
                    if (tag.HasElements) {
                        foreach (XElement curTag in tag.Elements()) {
                            Log.Warning();
                            Log.WriteLine($"Tag <{curTag.Name.ToString()}> no matched field. In {tag.BelongedDef().BaseInfo()}.");
                        }
                        nonMatchedElements.AddRange(tag.Elements());
                        tag.RemoveNodes();
                    }
                    foreach (XElement curTag in matchedSubTags) {
                        XComment curComment;
                        if (bufferComments.TryGetValue(curTag, out curComment)) {
                            tag.Add(curComment);
                        }
                        tag.Add(curTag);
                    }
                    tag.Add(nonMatchedElements);
                }
                break;
            case TagCategory.ListComplex: {
                    foreach (XElement curSubTag in tag.Elements()) {
                        string curSubTagClassName = curSubTag.GetClassName();
                        bool isMatched = false;
                        foreach (TagInfo curSubTagInfo in this.allTags.Values) {
                            if (curSubTagClassName == curSubTagInfo.className) {
                                isMatched = true;
                                curSubTagInfo.ProcessFieldNames(curSubTag);
                                break;
                            }
                        }
                        if (!isMatched) {
                            Log.Warning();
                            if (curSubTagClassName == null) {
                                Log.WriteLine($"Tag <li> no matched class. In <{curSubTag.Parent.Name}>. In {curSubTag.BelongedDef().BaseInfo()}.");
                            } else {
                                Log.WriteLine($"Tag <li class=\"{curSubTagClassName}\"> no matched class. In <{curSubTag.Parent.Name}>. In {curSubTag.BelongedDef().BaseInfo()}.");
                            }
                        }
                    }
                }
                break;
            case TagCategory.Def:
            case TagCategory.Enum:
            case TagCategory.FlagItem:
            case TagCategory.ListSimpleItem:
            case TagCategory.ListDefItem:
            case TagCategory.ListRefItem:
                Log.Warning();
                Log.WriteLine($"Tag '{tag.Name.ToString()}' has sub-elements, but matched invalid TagInfo (category: {this.tagCategory}).");
                break;
            case TagCategory.Flag:
            case TagCategory.ListSimple:
            case TagCategory.ListDef:
            case TagCategory.ListRef:
            case TagCategory.Dict:
            case TagCategory.DictItem:
            default:
                break;
            }
        }

        #endregion

    }
}
