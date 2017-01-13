using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RimTrans.Builder
{
    public class InjectionData : ILanguageData<InjectionData>
    {
        public InjectionData(DirectoryInfo injectionDir)
        {
            this._rootDir = injectionDir;
        }

        #region Info

        private DirectoryInfo _rootDir;
        public DirectoryInfo RootDir
        {
            get
            {
                return this._rootDir;
            }
        }

        #endregion

        #region Data

        private SortedDictionary<string, SortedDictionary<string, XDocument>> _dataBase = new SortedDictionary<string, SortedDictionary<string, XDocument>>();
        public SortedDictionary<string, SortedDictionary<string, XDocument>> DataBase
        {
            get
            {
                return this._dataBase;
            }
        }

        public int CountDoc
        {
            get
            {
                int count = 0;
                foreach (SortedDictionary<string, XDocument> docGroup in this._dataBase.Values)
                {
                    count += docGroup.Count;
                }
                return count;
            }
        }

        public int CountField
        {
            get
            {
                int count = 0;
                foreach (XDocument doc in from docGroup in this._dataBase.Values
                                          from doc in docGroup.Values
                                          select doc)
                {
                    count += doc.Root.Elements().Count();
                }
                return count;
            }
        }

        #endregion

        #region Loader and Parser

        public static InjectionData Load(DirectoryInfo injectionDir)
        {
            InjectionData injection = new InjectionData(injectionDir);

            injection._dataBase = new SortedDictionary<string, SortedDictionary<string, XDocument>>();
            if (injection._rootDir.Exists)
            {
                foreach (DirectoryInfo subdirectoryInfo in injection._rootDir.GetDirectories())
                {
                    SortedDictionary<string, XDocument> docGroup = new SortedDictionary<string, XDocument>();
                    int splitIndex = subdirectoryInfo.FullName.Length;
                    foreach (FileInfo fileInfo in subdirectoryInfo.GetFiles("*.xml", SearchOption.AllDirectories))
                    {
                        try
                        {
                            string key = fileInfo.FullName.Substring(splitIndex);
                            XDocument doc = RWXml.LoadLanguageDocument(fileInfo.FullName);
                            docGroup.Add(key, doc);
                        }
                        catch (XmlException ex)
                        {
                            TransLog.Message(injection, new TransLog.MessageArgs(
                                TransLog.Type.Error,
                                "InjectionData.Load " + fileInfo.FullName,
                                ex.Message));
                        }
                    }
                    if (docGroup.Count > 0)
                    {
                        injection._dataBase.Add(subdirectoryInfo.Name, docGroup);
                    }
                }
            }

            return injection;
        }

        public static InjectionData Parse(DefinitionData definition, DirectoryInfo injectionDir, bool isCore)
        {
            InjectionData injection = new InjectionData(injectionDir);

            foreach (KeyValuePair<string, XDocument> kvpFilePathDocument in definition.DataBase)
            {
                XDocument doc = kvpFilePathDocument.Value;
                string commentText = null;
                foreach (XNode node in doc.Root.Nodes())
                {
                    if (node.NodeType == XmlNodeType.Comment)
                    {
                        XComment comentCurrent = node as XComment;
                        string text = comentCurrent.Value.Trim().Trim('=').Trim();
                        if (text.Length > 0)
                        {
                            commentText = " " + text + " ";
                        }
                    }
                    else if (node.NodeType == XmlNodeType.Element &&
                        (node as XElement).Name != RWDefType.DutyDef.ToString() &&
                        (node as XElement).Name != RWDefType.EffecterDef.ToString() &&
                        (node as XElement).Name != RWDefType.GenStepDef.ToString() &&
                        (node as XElement).Name != RWDefType.MapGeneratorDef.ToString() &&
                        (node as XElement).Name != RWDefType.SongDef.ToString() &&
                        (node as XElement).Name != RWDefType.SoundDef.ToString() &&
                        (node as XElement).Name != RWDefType.ThinkTreeDef.ToString())
                    {
                        XElement def = node as XElement;
                        XElement defName = RWXml.GetField(def, RWFieldName.defName);
                        if (defName != null)
                        {
                            try
                            {
                                XElement temp = new XElement(defName.Value); // Check validity
                            }
                            catch (XmlException ex)
                            {
                                TransLog.Message(injection, new TransLog.MessageArgs(
                                    TransLog.Type.Error,
                                    "Invalid defName: " + defName.Value + " in file:" +defName.BaseUri,
                                    ex.Message));
                            }
                            finally
                            {
                                injection.AddFromDef(def, defName, Path.GetFileName(kvpFilePathDocument.Key), commentText);
                            }
                        }
                        
                    }
                }
            }
            injection.AddNelineAtEnd();
            if (isCore)
            {
                injection.AddTerrainAdd();
            }



            return injection;
        }

        #endregion

        #region Generater

        private XDocument GetDocument(string defType, string fileName)
        {
            string relativePath = Path.DirectorySeparatorChar + fileName;
            XDocument doc;
            SortedDictionary<string, XDocument> docGroup;
            if (this._dataBase.TryGetValue(defType, out docGroup))
            {
                if (docGroup.TryGetValue(relativePath, out doc))
                {
                }
                else
                {
                    doc = RWXml.EmptyDocumentSpecial();
                    docGroup.Add(relativePath, doc);
                }
            }
            else
            {
                doc = RWXml.EmptyDocumentSpecial();
                docGroup = new SortedDictionary<string, XDocument>();
                docGroup.Add(relativePath, doc);
                this._dataBase.Add(defType, docGroup);
            }
            return doc;
        }

        private void AddFromDef(XElement def, XElement defName, string fileName, string commentText)
        {
            bool isMote = false;
            bool isBuildable = false;
            bool isInstallable = false;
            bool isMakeable = false;
            bool isDrug = false;
            bool isPawn = false;
            RWDefType defType = RWXml.GetDefType(def);
            if (defType == RWDefType.ThingDef)
            {
                RWThingCategory category = RWXml.GetCategory(def);
                if (category == RWThingCategory.Mote)
                {
                    isMote = true;
                }
                else if (category == RWThingCategory.Building)
                {
                    if (RWXml.GetField(def, RWFieldName.designationCategory) != null) isBuildable = true;
                    if (RWXml.GetField(def, RWFieldName.minifiedDef) != null) isInstallable = true;
                    if (RWXml.GetField(def, RWFieldName.recipeMaker) != null) isMakeable = true;
                }
                else if (category == RWThingCategory.Item)
                {
                    if (RWXml.GetField(def, RWFieldName.recipeMaker) != null) isMakeable = true;
                    XElement ingestible = RWXml.GetField(def, RWFieldName.ingestible);
                    if (ingestible != null)
                    {
                        XElement drugCategory = RWXml.GetField(ingestible, RWFieldName.drugCategory);
                        if (drugCategory != null &&
                            string.Compare(drugCategory.Value, "None", true) != 0)
                        {
                            isDrug = true;
                        }
                    }
                }
                else if (category == RWThingCategory.Pawn)
                {
                    isPawn = true;
                }
            }
            else if (defType == RWDefType.TerrainDef && RWXml.GetField(def, RWFieldName.designationCategory) != null)
            {
                AddTerrainExtra(def, defName);
            }

            if (!isMote) // Note here!!!
            {
                // Generate fields
                LinkedList<XElement> linkedFields = new LinkedList<XElement>();
                linkedFields.AddLast(defName);
                IEnumerable<object> ToAdd = GenerateRecursively(def, linkedFields);
                if (isBuildable || isInstallable)
                {
                    ToAdd = ToAdd.Concat(GenerateBuildingExtra(def, defName, isBuildable, isInstallable));
                }
                if (isMakeable)
                {
                    this.AddRecipesAddMake(def, defName);
                }
                if (isDrug)
                {
                    this.AddRecipesAddAdminister(def, defName);
                }
                if (isPawn)
                {
                    ToAdd = ToAdd.Concat(GeneratePawnFlesh(def, defName));
                }
                int countFields = 0;
                foreach (object item in ToAdd)
                {
                    if (item is XElement) countFields++;
                }
                if (countFields > 0)
                {
                    // Get the document
                    XDocument doc = this.GetDocument(def.Name.ToString(), fileName);
                    // Add comment
                    XComment lastComment = null;
                    foreach (XNode node in doc.Root.Nodes())
                    {
                        if (node.NodeType == XmlNodeType.Comment)
                        {
                            lastComment = node as XComment;
                        }
                    }
                    if ((commentText != null && commentText != string.Empty) &&
                        (lastComment == null || lastComment.Value != commentText))
                    {
                        if (doc.Root.FirstAttribute.Value == "true")
                        {
                            doc.Root.Add("\r\n");
                            doc.Root.FirstAttribute.Value = "false";
                        }
                        if (doc.Root.HasElements) doc.Root.Add("\r\n");
                        doc.Root.Add(TransOption.Indent, new XComment(commentText), "\r\n\r\n");
                    }
                    // Add fields
                    if (countFields == 1)
                    {
                        doc.Root.FirstAttribute.Value = "true";
                        doc.Root.Add(ToAdd);
                    }
                    else if (countFields > 1)
                    {
                        if (doc.Root.FirstAttribute.Value == "true")
                        {
                            doc.Root.Add("\r\n");
                            doc.Root.FirstAttribute.Value = "false";
                        }
                        doc.Root.Add(ToAdd);
                        doc.Root.Add("\r\n");
                    }
                }
            }
        }

        private IEnumerable<object> GenerateRecursively(XElement field, LinkedList<XElement> linkedFields)
        {
            foreach (XElement field_Child in field.Elements())
            {
                if (field_Child.HasElements)
                {
                    linkedFields.AddLast(field_Child);
                    foreach (object item in GenerateRecursively(field_Child, linkedFields))
                    {
                        yield return item;
                    }
                    linkedFields.RemoveLast();
                }
                else if (RWXml.IsInjectable(field_Child))
                {
                    string injectionFieldName = "";
                    linkedFields.AddLast(field_Child);
                    foreach (XElement linkedField in linkedFields)
                    {
                        injectionFieldName += ".";
                        RWFieldName linkedFieldName = RWXml.GetFieldName(linkedField);
                        if (linkedFieldName == RWFieldName.defName)
                        {
                            injectionFieldName += linkedField.Value;
                        }
                        else if (linkedFieldName == RWFieldName.li)
                        {
                            injectionFieldName += linkedField.Attribute("RimTransIndex").Value;
                        }
                        else if (linkedFieldName == RWFieldName.Unknown)
                        {
                            injectionFieldName += linkedField.Name.ToString();
                        }
                        else
                        {
                            injectionFieldName += linkedFieldName.ToString();
                        }
                    }
                    linkedFields.RemoveLast();
                    injectionFieldName = injectionFieldName.Substring(1);
                    yield return TransOption.Indent;
                    yield return new XElement(injectionFieldName, field_Child.Value);
                    yield return "\r\n";
                }
            }
        }

        private IEnumerable<object> GenerateBuildingExtra(XElement def, XElement defName, bool isBuildable, bool isInstallable)
        {
            // Learn more in class ThingDefGenerator_Buildings of Assembly-CSharp.dll
            XElement label = RWXml.GetField(def, RWFieldName.label);
            if (label == null) label = defName;
            if (isBuildable)
            {
                yield return TransOption.Indent;
                yield return new XElement(defName.Value + "_Blueprint.label", label.Value + " (blueprint)");
                yield return "\r\n";
            }
            if (isInstallable)
            {
                yield return TransOption.Indent;
                yield return new XElement(defName.Value + "_Blueprint_Install.label", label.Value + " (blueprint)");
                yield return "\r\n";
            }
            if (isBuildable)
            {
                yield return TransOption.Indent;
                yield return new XElement(defName.Value + "_Frame.label", label.Value + " (building)");
                yield return "\r\n";
            }
        }

        private IEnumerable<object> GeneratePawnFlesh(XElement def, XElement defName)
        {
            XElement label = RWXml.GetField(def, RWFieldName.label);
            if (label == null) label = defName;

            bool isLeatherAmountZero = false;
            XElement statBases = RWXml.GetField(def, RWFieldName.statBases);
            if (statBases != null)
            {
                XElement leatherAmount = RWXml.GetField(statBases, RWFieldName.LeatherAmount);
                if (leatherAmount != null && leatherAmount.Value == "0") isLeatherAmountZero = true;
            }

            bool isFleshTypeMechanoid = false;
            XElement useLeatherFrom = null;
            XElement useMeatFrom = null;
            XElement leatherLabel = null;
            XElement meatLabel = null;
            XElement race = RWXml.GetField(def, RWFieldName.race);
            if (race != null)
            {
                XElement fleshType = RWXml.GetField(race, RWFieldName.fleshType);
                if (fleshType != null && fleshType.Value == "Mechanoid") isFleshTypeMechanoid = true;

                useLeatherFrom = RWXml.GetField(race, RWFieldName.useLeatherFrom);
                useMeatFrom = RWXml.GetField(race, RWFieldName.useMeatFrom);
                leatherLabel = RWXml.GetField(race, RWFieldName.leatherLabel);
                meatLabel = RWXml.GetField(race, RWFieldName.meatLabel);
            }

            if (isFleshTypeMechanoid)
            {
                yield return TransOption.Indent;
                yield return new XComment(" Flesh Type: Mechanoid ");
                yield return "\r\n";
            }
            else
            {
                // Leather
                if (isLeatherAmountZero)
                {
                    yield return TransOption.Indent;
                    yield return new XComment(" Leather Amount: 0 ");
                    yield return "\r\n";
                }
                else if (useLeatherFrom != null)
                {
                    yield return TransOption.Indent;
                    yield return new XComment(string.Format(" Use Leather From: {0} ", useLeatherFrom.Value));
                    yield return "\r\n";
                }
                else
                {
                    string leatherLabelValue =
                        leatherLabel == null ?
                        string.Format("{0} leather", label.Value) :
                        leatherLabel.Value;
                    yield return TransOption.Indent;
                    yield return new XElement(defName.Value + "_Leather.label", leatherLabelValue);
                    yield return "\r\n";
                    yield return TransOption.Indent;
                    yield return new XElement(defName.Value + "_Leather.description", leatherLabelValue);
                    yield return "\r\n";
                    yield return TransOption.Indent;
                    yield return new XElement(defName.Value + "_Leather.stuffProps.stuffAdjective", leatherLabelValue);
                    yield return "\r\n";
                }

                // Meat
                if (useMeatFrom != null)
                {
                    yield return TransOption.Indent;
                    yield return new XComment(string.Format(" Use Meat From: {0} ", useMeatFrom.Value));
                    yield return "\r\n";
                }
                else
                {
                    string meatLabelValue =
                        meatLabel == null ?
                        string.Format("{0} meat", label.Value) :
                        meatLabel.Value;
                    yield return TransOption.Indent;
                    yield return new XElement(defName.Value + "_Meat.label", meatLabelValue);
                    yield return "\r\n";
                    yield return TransOption.Indent;
                    yield return new XElement(defName.Value + "_Meat.description", meatLabelValue);
                    yield return "\r\n";
                }
            }

            // Corpse
            string corpseLabelValue = string.Format("{0} corpse", label.Value);
            yield return TransOption.Indent;
            yield return new XElement(defName.Value + "_Corpse.label", corpseLabelValue);
            yield return "\r\n";
            yield return TransOption.Indent;
            yield return new XElement(defName.Value + "_Corpse.description", corpseLabelValue);
            yield return "\r\n";
        }

        /// <summary>
        /// For Makeable ThinDef
        /// </summary>
        private void AddRecipesAddMake(XElement def, XElement defName)
        {
            XDocument doc = this.GetDocument(RWDefType.RecipeDef.ToString(), "Recipes_Add_Make.xml");
            bool isNew = true;
            foreach (XNode node in doc.Root.Nodes())
            {
                if (node.NodeType == XmlNodeType.Comment)
                {
                    isNew = false;
                    break;
                }
            }
            if (isNew)
            {
                doc.Root.Add(TransOption.Indent, new XComment(" SPECIAL: These Recipes from makeable Items and Buildings (ThingDef with <recipeMaker>), generated by RimTrans "), "\r\n\r\n");
            }

            XElement label = RWXml.GetField(def, RWFieldName.label);
            if (label == null) label = defName;

            XElement recipeMaker = RWXml.GetField(def, RWFieldName.recipeMaker);
            XElement recipeUsers = RWXml.GetField(recipeMaker, RWFieldName.recipeUsers);
            string users = "";
            if (recipeUsers != null)
            {
                foreach (XElement li in recipeUsers.Elements())
                {
                    users += ", " + li.Value;
                }
                users = " Recipe Users:" + users.Substring(1) + " ";

                XComment lastComment = null;
                foreach (XNode node in doc.Root.Nodes())
                {
                    if (node.NodeType == XmlNodeType.Comment) lastComment = node as XComment;
                }
                if (lastComment.Value != users)
                {
                    doc.Root.Add("\r\n  ", new XComment(users), "\r\n\r\n");
                }
            }

            // RecipeMake and RecipeMakeJobString in Misc_Gameplay.xml
            doc.Root.Add(TransOption.Indent, new XElement("Make_" + defName.Value + ".label", string.Format("Make {0}", label.Value)), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Make_" + defName.Value + ".description", string.Format("Make {0}.", label.Value)), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Make_" + defName.Value + ".jobString", string.Format("Making {0}.", label.Value)), "\r\n\r\n");
        }

        /// <summary>
        /// For drugs
        /// </summary>
        private void AddRecipesAddAdminister(XElement def, XElement defName)
        {
            XDocument doc = this.GetDocument(RWDefType.RecipeDef.ToString(), "Recipes_Add_Administer.xml");
            bool isNew = true;
            foreach (XNode node in doc.Root.Nodes())
            {
                if (node.NodeType == XmlNodeType.Comment)
                {
                    isNew = false;
                    break;
                }
            }
            if (isNew)
            {
                doc.Root.Add(TransOption.Indent, new XComment(" SPECIAL: These Recipes from Drugs (Items with <ingestible> and <drugCategory>), generated by RimTrans "), "\r\n\r\n");
            }

            XElement label = RWXml.GetField(def, RWFieldName.label);
            if (label == null) label = defName;

            // RecipeAdminister and RecipeAdministerJobString in Misc_Gameplay.xml
            doc.Root.Add(TransOption.Indent, new XElement("Administer_" + defName.Value + ".label", string.Format("Administer {0}", label.Value)), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Administer_" + defName.Value + ".jobString", string.Format("Administering {0}.", label.Value)), "\r\n\r\n");
        }

        private void AddTerrainExtra(XElement def, XElement defName)
        {
            XDocument doc = this.GetDocument(RWDefType.ThingDef.ToString(), "_Terrain_Extra.xml");
            bool isNew = true;
            foreach (XNode node in doc.Root.Nodes())
            {
                if (node.NodeType == XmlNodeType.Comment)
                {
                    isNew = false;
                    break;
                }
            }
            if (isNew)
            {
                doc.Root.Add(TransOption.Indent, new XComment(" SPECIAL: Blueprint and Frame of Terrian, generated by RimTrans "), "\r\n\r\n");
            }

            XElement label = RWXml.GetField(def, RWFieldName.label);
            if (label == null) label = defName;

            doc.Root.Add(TransOption.Indent, new XElement(defName.Value + "_Blueprint.label", label.Value + " (blueprint)"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement(defName.Value + "_Frame.label", label.Value + " (building)"), "\r\n\r\n");
        }

        /// <summary>
        /// For vanilla natural stone floor
        /// </summary>
        private void AddTerrainAdd()
        {

            // Get the document
            XDocument doc = this.GetDocument(RWDefType.TerrainDef.ToString(), "Terrain_Add.xml");
            doc.Root.RemoveAttributes();
            doc.Root.Add(TransOption.Indent, new XComment(" SPECIAL: Floors from natural stones "), "\r\n\r\n");

            doc.Root.Add(TransOption.Indent, new XElement("Sandstone_Rough.label", "rough sandstone"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Sandstone_RoughHewn.label", "rough-hewn sandstone"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Sandstone_Smooth.label", "smooth sandstone"), "\r\n\r\n");

            doc.Root.Add(TransOption.Indent, new XElement("Granite_Rough.label", "rough granite"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Granite_RoughHewn.label", "rough-hewn granite"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Granite_Smooth.label", "smooth granite"), "\r\n\r\n");

            doc.Root.Add(TransOption.Indent, new XElement("Limestone_Rough.label", "rough limestone"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Limestone_RoughHewn.label", "rough-hewn limestone"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Limestone_Smooth.label", "smooth limestone"), "\r\n\r\n");

            doc.Root.Add(TransOption.Indent, new XElement("Slate_Rough.label", "rough slate"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Slate_RoughHewn.label", "rough-hewn slate"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Slate_Smooth.label", "smooth slate"), "\r\n\r\n");

            doc.Root.Add(TransOption.Indent, new XElement("Marble_Rough.label", "rough marble"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Marble_RoughHewn.label", "rough-hewn marble"), "\r\n");
            doc.Root.Add(TransOption.Indent, new XElement("Marble_Smooth.label", "smooth marble"), "\r\n\r\n");

            doc.Root.Add("\r\n");
        }

        private void AddNelineAtEnd()
        {
            foreach (SortedDictionary<string, XDocument> docGroup in this._dataBase.Values)
            {
                foreach (XDocument doc in docGroup.Values)
                {
                    if (doc.Root.FirstAttribute.Value == "true")
                    {
                        doc.Root.Add("\r\n\r\n");
                    }
                    else
                    {
                        doc.Root.Add("\r\n");
                    }
                    doc.Root.RemoveAttributes();
                }
            }
        }

        #endregion

        #region Matcher

        /// <summary>
        /// Match new fields to existing fields and continue to use, comment those invalid old fields 
        /// </summary>
        /// <param name="injectionExisting"></param>
        private void MatchExisting(InjectionData injectionExisting)
        {

            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> kvpDefTypeDocGroupExisting in injectionExisting._dataBase)
            {
                SortedDictionary<string, XDocument> docGroupExisting = kvpDefTypeDocGroupExisting.Value;
                SortedDictionary<string, XDocument> docGroupThis;
                if (this._dataBase.TryGetValue(kvpDefTypeDocGroupExisting.Key, out docGroupThis))
                {
                    foreach (KeyValuePair<string, XDocument> kvpRelativePathDocumentExisting in docGroupExisting)
                    {
                        XDocument docExisting = kvpRelativePathDocumentExisting.Value;
                        XDocument docThis;
                        if (docGroupThis.TryGetValue(kvpRelativePathDocumentExisting.Key, out docThis))
                        {
                            int countInvalid = 0;
                            foreach (XNode nodeExisting in docExisting.Root.Nodes())
                            {
                                if (nodeExisting.NodeType == XmlNodeType.Comment)
                                {
                                    bool isInvalid = true;
                                    foreach (XNode nodeThis in docThis.Root.Nodes())
                                    {
                                        if (nodeThis.NodeType == XmlNodeType.Comment &&
                                            (nodeExisting as XComment).Value == (nodeThis as XComment).Value)
                                        {
                                            isInvalid = false;
                                            break;
                                        }
                                    }
                                    if (isInvalid)
                                    {
                                        docThis.Root.Add(TransOption.Indent, nodeExisting, "\r\n");
                                        countInvalid++;
                                    }
                                }
                                else if (nodeExisting.NodeType == XmlNodeType.Element)
                                {
                                    bool isInvalid = true;
                                    foreach (XElement fieldThis in docThis.Root.Elements())
                                    {
                                        if (RWXml.CompareFieldsName((nodeExisting as XElement), fieldThis))
                                        {
                                            isInvalid = false;
                                            break;
                                        }
                                    }
                                    if (isInvalid)
                                    {
                                        docThis.Root.Add(TransOption.Indent, new XComment((nodeExisting as XElement).ToString()), "\r\n");
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
                            docGroupThis.Add(kvpRelativePathDocumentExisting.Key, RWXml.DocumentCommentAll(docExisting));
                        }
                    }
                }
                else
                {
                    docGroupThis = new SortedDictionary<string, XDocument>();
                    foreach (KeyValuePair<string, XDocument> kvpRelatviePathDocumentExisting in docGroupExisting)
                    {
                        docGroupThis.Add(kvpRelatviePathDocumentExisting.Key, RWXml.DocumentCommentAll(kvpRelatviePathDocumentExisting.Value));
                    }
                    this._dataBase.Add(kvpDefTypeDocGroupExisting.Key, docGroupThis);
                }
            }
            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> kvpDefTypeDocGroup in this._dataBase)
            {
                SortedDictionary<string, XDocument> docGroupThis = kvpDefTypeDocGroup.Value;
                SortedDictionary<string, XDocument> docGroupExisting;
                if (injectionExisting._dataBase.TryGetValue(kvpDefTypeDocGroup.Key, out docGroupExisting))
                {
                    foreach (XElement fieldThis in from doc in docGroupThis.Values
                                                   from ele in doc.Root.Elements()
                                                   select ele)
                    {
                        foreach (XElement fieldExisting in from doc in docGroupExisting.Values
                                                           from ele in doc.Root.Elements()
                                                           select ele)
                        {
                            if (RWXml.CompareFieldsName(fieldThis, fieldExisting))
                            {
                                //Console.WriteLine(fieldThis.Value + fieldExisting.Value);
                                fieldThis.Value = fieldExisting.Value;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Comment the redundant fields
        /// </summary>
        private void MatchSelf()
        {
            foreach (SortedDictionary<string, XDocument> docGroup in this._dataBase.Values)
            {
                List<XElement> fields = new List<XElement>();
                fields.AddRange(from doc in docGroup.Values
                                from ele in doc.Root.Elements()
                                select ele);
                for (int i = 0; i < fields.Count; i++)
                {
                    XElement field_i = fields[i];
                    for (int j = i + 1; j < fields.Count; j++)
                    {
                        XElement field_j = fields[j];
                        if (RWXml.CompareFieldsName(field_i, field_j))
                        {
                            field_i.ReplaceWith(new XComment(string.Format("[Duplicate] {0}", field_i.ToString())));
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Comment the fields conflicted to Core
        /// </summary>
        private void MatchCore(InjectionData injectionCore)
        {
            List<XElement> fieldsConflicted = new List<XElement>();
            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> kvpDefTypeDocGroupThis in this._dataBase)
            {
                SortedDictionary<string, XDocument> docGroupThis = kvpDefTypeDocGroupThis.Value;
                SortedDictionary<string, XDocument> docGroupCore;
                if (injectionCore._dataBase.TryGetValue(kvpDefTypeDocGroupThis.Key, out docGroupCore))
                {
                    IEnumerable<XElement> fieldsThis = from doc in docGroupThis.Values
                                                       from ele in doc.Root.Elements()
                                                       select ele;
                    IEnumerable<XElement> fieldsCore = from doc in docGroupCore.Values
                                                       from ele in doc.Root.Elements()
                                                       select ele;
                    foreach (XElement fieldThis in fieldsThis)
                    {
                        foreach (XElement fieldCore in fieldsCore)
                        {
                            if (RWXml.CompareFieldsName(fieldThis, fieldCore))
                            {
                                fieldsConflicted.Add(fieldThis);
                            }
                        }
                    }
                }
            }
            foreach (XElement field in fieldsConflicted)
            {
                field.ReplaceWith(new XComment(string.Format("[Core] {0}", field.ToString())));
            }
        }

        private void RemoveEmptyDoc()
        {
            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> kvpDefTypeDocGroup in this._dataBase)
            {
                List<string> EmptyDocs = new List<string>();
                foreach (KeyValuePair<string, XDocument> kvpRelativePathDocument in kvpDefTypeDocGroup.Value)
                {
                    bool isEmpty = true;
                    foreach (XNode node in kvpRelativePathDocument.Value.Root.Nodes())
                    {
                        if (node.NodeType == XmlNodeType.Comment || node.NodeType == XmlNodeType.Element)
                        {
                            isEmpty = false;
                            break;
                        }
                    }
                    if (isEmpty)
                    {
                        EmptyDocs.Add(kvpRelativePathDocument.Key);
                    }
                }
                foreach (string key in EmptyDocs)
                {
                    kvpDefTypeDocGroup.Value.Remove(key);
                }
            }

            List<string> EmptyDocGroups = new List<string>();
            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> kvpDefTypeDocGroup in this._dataBase)
            {
                if (kvpDefTypeDocGroup.Value.Count == 0)
                {
                    EmptyDocGroups.Add(kvpDefTypeDocGroup.Key);
                }
            }
            foreach (string key in EmptyDocGroups)
            {
                this._dataBase.Remove(key);
            }
        }

        #endregion

        #region Interface

        public InjectionData BuildNew(InjectionData injectionOriginal, bool isFreshBuild, InjectionData injectionCore = null)
        {
            InjectionData injectionNew = new InjectionData(this._rootDir);

            injectionNew._dataBase = new SortedDictionary<string, SortedDictionary<string, XDocument>>();
            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> kvpDefTypeDocGroup in injectionOriginal._dataBase)
            {
                SortedDictionary<string, XDocument> docGroupNew = new SortedDictionary<string, XDocument>();
                foreach (KeyValuePair<string, XDocument> docGroup in kvpDefTypeDocGroup.Value)
                {
                    docGroupNew.Add(docGroup.Key, new XDocument(docGroup.Value));
                }
                injectionNew._dataBase.Add(kvpDefTypeDocGroup.Key, docGroupNew);
            }

            if (!isFreshBuild)
            {
                injectionNew.MatchExisting(this);
            }
            injectionNew.MatchSelf();
            if (injectionCore != null)
            {
                injectionNew.MatchCore(injectionCore);
            }
            injectionNew.RemoveEmptyDoc();

            return injectionNew;
        }

        public void Save()
        {
            Save(this._rootDir.FullName);
        }

        public void Save(string path)
        {
            if (this.CountDoc > 0)
            {
                foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> kvpDefTypeDocGroup in this._dataBase)
                {
                    string defTypePath = Path.Combine(path, kvpDefTypeDocGroup.Key);

                    foreach (KeyValuePair<string, XDocument> kvpRelativePathDocument in kvpDefTypeDocGroup.Value)
                    {
                        string filePath = defTypePath + kvpRelativePathDocument.Key;
                        DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(filePath));
                        if (!dir.Exists) dir.Create();

                        if (kvpDefTypeDocGroup.Key == RWDefType.InteractionDef.ToString() ||
                            kvpDefTypeDocGroup.Key == RWDefType.RulePackDef.ToString() ||
                            kvpDefTypeDocGroup.Key == RWDefType.TaleDef.ToString())
                        {
                            string text = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n";
                            text += kvpRelativePathDocument.Value.Root.ToString();
                            text = text.Replace("-&gt;", "->");
                            using (StreamWriter sw = new StreamWriter(filePath))
                            {
                                sw.Write(text);
                            }
                        }
                        else
                        {
                            kvpRelativePathDocument.Value.Save(filePath);
                        }
                    }
                }
            }
        }

        #endregion

    }
}
