using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RimTrans.Builder.Xml;

namespace RimTrans.Builder {
    public class InjectionData {
        #region Language Code




        #endregion

        private SortedDictionary<string, SortedDictionary<string, XDocument>> _data;
        public SortedDictionary<string, SortedDictionary<string, XDocument>> Data { get { return this._data; } }

        public string Name { get; private set; }

        public string Code {
            get {
                return LangCodeHelper.GetCode(this.Name);
            }
        }

        #region Methods

        public IEnumerable<XElement> Injections(string defType, string defName) {
            if (this._data != null) {
                SortedDictionary<string, XDocument> subData;
                if (this._data.TryGetValue(defType, out subData)) {
                    foreach (XElement curInjection in from doc in subData.Values
                                                      from ele in doc.Root.Elements()
                                                      select ele) {
                        string path = curInjection.Name.ToString();
                        if (path.Substring(0, path.IndexOf('.')) == defName) {
                            yield return curInjection;
                        }
                    }
                }
            }
        }

        #endregion


        private InjectionData() {

        }

        // Clone a InjectionData object
        public InjectionData(string name, InjectionData other) {
            this.Name = name;
            this._data = new SortedDictionary<string, SortedDictionary<string, XDocument>>();
            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> defTypeNameSubDataPairOther in other._data) {
                string defTypeName = defTypeNameSubDataPairOther.Key;
                SortedDictionary<string, XDocument> subData = new SortedDictionary<string, XDocument>();
                SortedDictionary<string, XDocument> subDataOther = defTypeNameSubDataPairOther.Value;
                foreach (KeyValuePair<string, XDocument> fileNameDocOther in subDataOther) {
                    subData.Add(fileNameDocOther.Key, new XDocument(fileNameDocOther.Value));
                }
                this._data.Add(defTypeName, subData);
            }
        }


        #region Load

        /// <summary>
        /// Load existed DefInjected
        /// </summary>
        public static InjectionData Load(string name, string path, bool backupInvalidFile = false) {
            InjectionData injectionData = new InjectionData();
            injectionData.Name = name;
            injectionData._data = new SortedDictionary<string, SortedDictionary<string, XDocument>>();

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists) {
                Log.Info();
                Log.Write("Loading DefInjected: ");
                Log.WriteLine(ConsoleColor.Cyan, path);
                int countValidFiles = 0;
                int countInvalidFiles = 0;
                foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories()) {
                    SortedDictionary<string, XDocument> subData = new SortedDictionary<string, XDocument>();
                    int splitIndex = subDirInfo.FullName.Length + 1;
                    foreach (FileInfo fileInfo in subDirInfo.GetFiles("*.xml", SearchOption.AllDirectories)) {
                        XDocument doc = null;
                        string filePath = fileInfo.FullName;
                        try {
                            doc = DocHelper.LoadLanguageDoc(filePath);
                            countValidFiles++;
                        } catch (XmlException ex) {
                            Log.Error();
                            Log.Write("Loading file failed: ");
                            Log.WriteLine(ConsoleColor.Red, filePath);
                            Log.Indent();
                            Log.WriteLine(ex.Message);
                            if (backupInvalidFile) {
                                try {
                                    string backupFile = filePath + ".BAK";
                                    fileInfo.CopyTo(backupFile, true);
                                    Log.Indent();
                                    Log.Write("Having been backed up to: ");
                                    Log.WriteLine(ConsoleColor.Yellow, backupFile);
                                } catch (Exception) {
                                    Log.Error();
                                    Log.WriteLine("Backing up failed.");
                                    throw;
                                }
                            }
                            countInvalidFiles++;
                        }
                        if (doc != null) {
                            subData.Add(filePath.Substring(splitIndex), doc);
                        }
                    }
                    if (subData.Count() > 0) {
                        injectionData._data.Add(subDirInfo.Name, subData);
                    }
                }
                if (countValidFiles > 0) {
                    if (countInvalidFiles == 0) {
                        Log.Info();
                        Log.WriteLine("Completed Loading DefInjected: {0} file(s).", countValidFiles);
                    } else {
                        Log.Warning();
                        Log.WriteLine("Completed Loading DefInjected: Success: {0} file(s), Failure: {1} file(s).", countValidFiles, countInvalidFiles);
                    }
                } else {
                    if (countInvalidFiles == 0) {
                        Log.Info();
                        Log.WriteLine("Directory \"DefInjected\" is empty.");
                    } else {
                        Log.Error();
                        Log.WriteLine("Loading failed: {1} file(s).", countInvalidFiles);
                    }
                }
            } else {
                Log.Info();
                Log.Write("Directory \"DefInjected\" does not exist: ");
                Log.WriteLine(ConsoleColor.Cyan, path);
            }
            return injectionData;
        }

        #endregion

        #region Parse

        /// <summary>
        /// Parse Defs to generate original DefInjected
        /// </summary>
        public static InjectionData Parse(string name, DefinitionData definitionData) {
            InjectionData injectionData = new InjectionData();
            injectionData._data = new SortedDictionary<string, SortedDictionary<string, XDocument>>();

            SortedDictionary<string, XDocument> definitions = definitionData.Data;
            if (definitions.Count() > 0) {
                Log.Info();
                Log.Write("Start parsing Defs and generating ");
                Log.Write(ConsoleColor.Cyan, "Original DefInjected");
                Log.WriteLine(".");
                int countValidDefs = 0;
                int countInvalidDefs = 0;
                int countFields = 0;
                foreach (KeyValuePair<string, XDocument> pathDocPair in definitions) {
                    string commentText = null;
                    foreach (XNode node in pathDocPair.Value.Root.Nodes()) {
                        if (node.NodeType == XmlNodeType.Comment) {
                            XComment curComment = node as XComment;
                            string text = curComment.Value.Trim().Trim('=').Trim();
                            if (text.Length > 0 /*&& !text.Contains("\n") && !text.Contains("\r")*/) {
                                commentText = " " + text + " ";
                            }
                        } else if (node.NodeType == XmlNodeType.Element && (node as XElement).IsNeedToTranslate()) {
                            XElement def = node as XElement;
                            XElement defName = def.defName();
                            if (defName != null) {
                                bool isValid = false;
                                try {
                                    XElement tempEle = new XElement(defName.Value);
                                    isValid = true;
                                } catch (XmlException ex) {
                                    Log.Error();
                                    Log.Write("Invalid ");
                                    Log.Write(ConsoleColor.Red, "<defName>{0}</defName>", defName.Value);
                                    Log.Write(": ");
                                    Log.WriteLine(ex.Message);
                                    Log.Indent();
                                    Log.WriteLine(ConsoleColor.Red, defName.BaseUri);
                                    countInvalidDefs++;
                                }
                                if (isValid) {
                                    int count = injectionData.AddFromDef(def, pathDocPair, defName, commentText);
                                    if (count > 0) {
                                        countValidDefs++;
                                        countFields += count;
                                    }
                                }
                            }
                        }
                    }
                }
                if (countFields > 0) {
                    injectionData.Tidy();
                    //if (isCore)
                    //{
                    //    // Something for core
                    //}
                    if (countInvalidDefs > 0) {
                        Log.Error();
                        Log.WriteLine("Encountered {0} invalid Defs during parsing. Other Defs have been parsed successfully", countInvalidDefs);
                    }
                    Log.Info();
                    Log.WriteLine("Completed generating DefInjected: {0} Def node(s) -> {1} DefInjected node(s).", countValidDefs, countFields);
                } else if (countInvalidDefs > 0) {
                    Log.Error();
                    Log.WriteLine("Generating DefInjected failed: {0} invalid Def(s).", countInvalidDefs);
                } else {
                    Log.Info();
                    Log.WriteLine("Completed parsing and no DefInjected to be generated.");
                }
            }

            return injectionData;
        }

        #endregion

        #region Get Doc

        private XDocument GetDoc(string defTypeName, string fileName) {
            XDocument doc;
            SortedDictionary<string, XDocument> subData;
            if (this._data.TryGetValue(defTypeName, out subData)) {
                if (!subData.TryGetValue(fileName, out doc)) {
                    doc = DocHelper.EmptyDoc();
                    subData.Add(fileName, doc);
                }
            } else {
                doc = DocHelper.EmptyDoc();
                subData = new SortedDictionary<string, XDocument>();
                subData.Add(fileName, doc);
                this._data.Add(defTypeName, subData);
            }
            return doc;
        }

        private XDocument GetDocEx(string defTypeName, string fileName) {
            XDocument doc;
            SortedDictionary<string, XDocument> subData;
            if (this._data.TryGetValue(defTypeName, out subData)) {
                if (!subData.TryGetValue(fileName, out doc)) {
                    doc = DocHelper.EmptyDocEx();
                    subData.Add(fileName, doc);
                }
            } else {
                doc = DocHelper.EmptyDocEx();
                subData = new SortedDictionary<string, XDocument>();
                subData.Add(fileName, doc);
                this._data.Add(defTypeName, subData);
            }
            return doc;
        }

        #endregion

        #region Generate and Add

        /// <summary>
        /// Generate DefInjected contents, (universal)
        /// </summary>
        private int AddFromDef(XElement def, KeyValuePair<string, XDocument> pathDocPair, XElement defName, string commentText) {
            int result = 0;

            bool isMote = false;
            bool isBuildable = false;
            bool isMinifiable = false;
            bool isMakeable = false;
            bool isDrug = false;
            bool isPawn = false;

            string defTypeName = def.Name.ToString();
            if (defTypeName == DefTypeNameOf.ThingDef) {
                XElement category = def.Field(FieldNameOf.category);
                if (category != null) {
                    XElement recipeMaker = def.Field(FieldNameOf.recipeMaker);
                    if (recipeMaker != null && recipeMaker.HasElements)
                        isMakeable = true;
                    string thingCategory = category.Value;
                    if (thingCategory == ThingCategoryOf.Mote) {
                        isMote = true;
                        this.AddThingsMote(def, defName);
                        result++;
                    } else if (thingCategory == ThingCategoryOf.Building) {
                        XElement designationCategory = def.Field(FieldNameOf.designationCategory);
                        XElement minifiedDef = def.Field(FieldNameOf.minifiedDef);
                        if (designationCategory != null && !string.IsNullOrWhiteSpace(designationCategory.Value))
                            isBuildable = true;
                        if (minifiedDef != null && !string.IsNullOrWhiteSpace(minifiedDef.Value))
                            isMinifiable = true;
                        XElement building = def.Field(FieldNameOf.building);
                        if (building != null) {
                            XElement isNaturalRock = building.Field(FieldNameOf.isNaturalRock);
                            XElement isResourceRock = building.Field(FieldNameOf.isResourceRock);
                            if (isNaturalRock != null && string.Compare(isNaturalRock.Value, "true", true) == 0 &&
                                (isResourceRock == null || string.Compare(isResourceRock.Value, "false", true) == 0)) {
                                this.AddTerrainAdd(def, defName);
                                result += 6;
                            }
                        }
                    } else if (thingCategory == ThingCategoryOf.Item) {
                        XElement ingestible = def.Field(FieldNameOf.ingestible);
                        if (ingestible != null && ingestible.HasElements) {
                            XElement drugCategory = ingestible.Field(FieldNameOf.drugCategory);
                            if (drugCategory != null &&
                                drugCategory.Value != "None") {
                                isDrug = true;
                            }
                        }
                    } else if (thingCategory == ThingCategoryOf.Pawn) {
                        isPawn = true;
                    }
                }
            } else if (defTypeName == DefTypeNameOf.DesignationCategoryDef) {
                this.AddKeyBindingCategoriesAddArchitect(def, defName);
                result += 2;
            } else if (defTypeName == DefTypeNameOf.MainTabDef || defTypeName == DefTypeNameOf.MainButtonDef) {
                if (def.Field(FieldNameOf.defaultToggleKey) != null || def.Field(FieldNameOf.defaultHotKey) != null) {
                    this.AddKeyBindingsAddMainTab(def, defName);
                    result++;
                }
            } else if (defTypeName == DefTypeNameOf.TrainableDef) {
                this.AddPawnColumn(def, defName);
                result++;
            } else if (defTypeName == DefTypeNameOf.TerrainDef && def.Field(FieldNameOf.designationCategory) != null) {
                this.AddTerrainExtra(def, defName);
                result += 2;
            }

            // Mote does not need to translate.
            if (!isMote) {
                LinkedList<XElement> fieldPath = new LinkedList<XElement>();
                fieldPath.AddLast(defName);
                IEnumerable<object> contents = GenRecursively(def, fieldPath); // Recursively
                if (isBuildable || isMinifiable) {
                    contents = contents.Concat(GenExtraBuilding(def, defName, isBuildable, isMinifiable));
                }
                if (isPawn) {
                    contents = contents.Concat(GenExtraPawn(def, defName));
                }
                int countFields = 0;
                foreach (object item in contents) {
                    if (item is XElement)
                        countFields++;
                }
                if (countFields > 0) {
                    XDocument doc = this.GetDocEx(def.Name.ToString(), Path.GetFileName(pathDocPair.Key));
                    XElement root = doc.Root;
                    XComment lastComment = null;
                    foreach (XNode node in root.Nodes()) {
                        if (node.NodeType == XmlNodeType.Comment)
                            lastComment = node as XComment;
                    }
                    XAttribute layout = root.FirstAttribute;
                    if (commentText != null &&
                        (lastComment == null || lastComment.Value != commentText)) {
                        if (layout.Value == "true") {
                            layout.Value = "false";
                            if (root.HasElements)
                                root.Add("\n\n");
                        } else {
                            if (root.HasElements)
                                root.Add("\n");
                        }
                        root.Add("  ", new XComment(commentText), "\n\n");
                    }
                    if (countFields == 1) {
                        layout.Value = "true";
                        root.Add(contents);
                    } else {
                        if (layout.Value == "true") {
                            layout.Value = "false";
                            root.Add("\n");
                        }
                        root.Add(contents);
                        root.Add("\n");
                    }
                }
                if (isMakeable) {
                    this.AddRecipesAddMake(def, defName);
                    result += 3;
                }
                if (isDrug) {
                    this.AddRecipesAddAdminister(def, defName);
                    result += 2;
                }
                result += countFields;
            }
            return result;
        }

        /// <summary>
        /// Generate DefInjected contents, (recursively)
        /// </summary>
        private static IEnumerable<object> GenRecursively(XElement field, LinkedList<XElement> fieldPath) {
            foreach (XElement fieldChild in field.Elements()) {
                if (fieldChild.HasElements) {
                    fieldPath.AddLast(fieldChild);
                    foreach (object content in GenRecursively(fieldChild, fieldPath)) {
                        yield return content;
                    }
                    fieldPath.RemoveLast();
                } else if (fieldChild.IsInjectable() || fieldChild.IsInjectableExtra()) {
                    StringBuilder fullFieldName = new StringBuilder();
                    bool isDefName = true;
                    foreach (XElement linkedField in fieldPath) {
                        if (isDefName) {
                            fullFieldName.Append(linkedField.Value);
                            isDefName = false;
                        } else if (linkedField.Name == "li") {
                            fullFieldName.Append(linkedField.Attribute("ListIndex").Value);
                        } else {
                            fullFieldName.Append(linkedField.Name.ToString());
                        }
                        fullFieldName.Append('.');
                    }

                    //XNode previousNode = fieldChild.PreviousNode;
                    //if (previousNode != null && previousNode.NodeType == XmlNodeType.Comment)
                    //{
                    //    yield return "  ";
                    //    yield return previousNode;
                    //    yield return "\n";
                    //}

                    string fieldChildName = fieldChild.Name.ToString();
                    fullFieldName.Append(fieldChildName == "li" ?
                        fieldChild.Attribute("ListIndex").Value :
                        fieldChildName);
                    yield return "  ";
                    yield return new XElement(fullFieldName.ToString(), fieldChild.Value);
                    yield return "\n";
                }
            }
        }

        /// <summary>
        /// For blueprint and frame from buildable and minifiable builings.
        /// </summary>
        private static IEnumerable<object> GenExtraBuilding(XElement def, XElement defName, bool isBuildable, bool isMinifiable) {
            XElement label = def.label();
            if (label == null)
                label = defName;
            XElement description = def.description();
            if (description == null)
                description = label;

            // Content generation in Keyed/Misc.xml
            if (isBuildable) {
                yield return "  ";
                yield return new XElement(defName.Value + "_Blueprint.label", label.Value + " (blueprint)");
                yield return "\n";
            }
            if (isMinifiable) {
                yield return "  ";
                yield return new XElement(defName.Value + "_Blueprint_Install.label", label.Value + " (blueprint)");
                yield return "\n";
            }
            if (isBuildable) {
                yield return "  ";
                yield return new XElement(defName.Value + "_Frame.label", label.Value + " (building)");
                yield return "\n";
                yield return "  ";
                yield return new XElement(defName.Value + "_Frame.description", description.Value);
                yield return "\n";
            }
        }

        /// <summary>
        /// For meat, leather and corpse from pawns.
        /// </summary>
        private static IEnumerable<object> GenExtraPawn(XElement def, XElement defName) {
            XElement label = def.label();
            if (label == null)
                label = defName;
            string defNameValue = defName.Value;
            string labelValue = label.Value;

            XElement butcherProducts = def.Field(FieldNameOf.butcherProducts);
            XElement LeatherAmount = null;
            XElement MeatAmount = null;
            XElement statBases = def.Field(FieldNameOf.statBases);
            if (statBases != null) {
                LeatherAmount = statBases.Element("LeatherAmount");
                MeatAmount = statBases.Element("MeatAmount");
            }
            XElement fleshType = null;
            XElement useLeatherFrom = null;
            XElement useMeatFrom = null;
            XElement leatherLabel = null;
            XElement meatLabel = null;
            XElement race = def.Field(FieldNameOf.race);
            if (race != null) {
                fleshType = race.Field(FieldNameOf.fleshType);
                useLeatherFrom = race.Field(FieldNameOf.useLeatherFrom);
                useMeatFrom = race.Field(FieldNameOf.useMeatFrom);
                leatherLabel = race.Field(FieldNameOf.leatherLabel);
                meatLabel = race.Field(FieldNameOf.meatLabel);
            }

            // Content generation in Keyed/Misc.xml
            if (fleshType != null && fleshType.Value == "Mechanoid") {
                yield return "  ";
                yield return new XComment(" Flesh Type: Mechanoid ");
                yield return "\n";
            } else {
                // Leather
                if (butcherProducts != null) {
                    yield return "  ";
                    yield return new XComment(" No Leather: Butcher Products ");
                    yield return "\n";
                } else if (LeatherAmount != null && LeatherAmount.Value == "0") {
                    yield return "  ";
                    yield return new XComment(" Leather Amount: 0 ");
                    yield return "\n";
                } else if (useLeatherFrom != null) {
                    yield return "  ";
                    yield return new XComment(string.Format(" Use Leather From: {0} ", useLeatherFrom.Value));
                    yield return "\n";
                } else {
                    string leatherLabelValue =
                        leatherLabel == null ?
                        string.Format("{0} leather", labelValue) :
                        leatherLabel.Value;
                    yield return "  ";
                    yield return new XElement(defNameValue + "_Leather.label", leatherLabelValue);
                    yield return "\n";
                    yield return "  ";
                    yield return new XElement(defNameValue + "_Leather.description", leatherLabelValue);
                    yield return "\n";
                    yield return "  ";
                    yield return new XElement(defNameValue + "_Leather.stuffProps.stuffAdjective", leatherLabelValue);
                    yield return "\n";
                }

                // Meat
                if (MeatAmount != null && MeatAmount.Value == "0") {
                    yield return "  ";
                    yield return new XComment(" Meat Amount: 0 ");
                    yield return "\n";
                } else if (useMeatFrom != null) {
                    yield return "  ";
                    yield return new XComment(string.Format(" Use Meat From: {0} ", useMeatFrom.Value));
                    yield return "\n";
                } else {
                    string meatLabelValue = meatLabel == null ? string.Format("{0} meat", labelValue) : meatLabel.Value;
                    yield return "  ";
                    yield return new XElement(defNameValue + "_Meat.label", meatLabelValue);
                    yield return "\n";
                    yield return "  ";
                    yield return new XElement(defNameValue + "_Meat.description", meatLabelValue);
                    yield return "\n";
                }
            }

            // Corpse
            string corpseLabelValue = string.Format("{0} corpse", labelValue);
            yield return "  ";
            yield return new XElement(defNameValue + "_Corpse.label", corpseLabelValue);
            yield return "\n";
            yield return "  ";
            yield return new XElement(defNameValue + "_Corpse.description", corpseLabelValue);
            yield return "\n";
        }

        private void AddPawnColumn(XElement def, XElement defName) {
            XDocument doc = this.GetDocEx(DefTypeNameOf.PawnColumnDef, "PawnColumns_Add.xml");
            XElement root = doc.Root;
            if (!root.HasElements) {
                root.Add("  ", new XComment(" SPECIAL: These Pawn Columns from TrainableDefs, generated by RimTrans "), "\n\n");
                root.FirstAttribute.Value = "true";
            }

            XElement label = def.label();
            if (label == null)
                label = defName;
            root.Add("  ", new XElement("Trainable_" + defName.Value + ".headerTip", label.Value), "\n");
        }

        private void AddThingsMote(XElement def, XElement defName) {
            XDocument doc = this.GetDocEx(DefTypeNameOf.ThingDef, "_Things_Mote.xml");
            XElement root = doc.Root;
            if (!root.HasElements) {
                root.Add("  ", new XComment(" These are Mote ThingDefs. Remain the same, DO NOT translate them. "), "\n\n");
                root.FirstAttribute.Value = "true";
            }

            XElement label = def.label();
            if (label != null) {
                root.Add("  ", new XElement(defName.Value + ".label", label.Value), "\n");
            }
        }

        /// <summary>
        /// For manufacture recipe from makeable items and buildings 
        /// </summary>
        private void AddRecipesAddMake(XElement def, XElement defName) {
            XDocument doc = this.GetDoc(DefTypeNameOf.RecipeDef, "Recipes_Add_Make.xml");
            XElement root = doc.Root;
            if (!root.HasElements) {
                root.Add("  ", new XComment(" SPECIAL: These Recipes from makeable ThingDefs (which include <recipeMaker>), generated by RimTrans "), "\n\n");
            }

            XElement label = def.label();
            if (label == null)
                label = defName;

            XElement recipeMaker = def.Field(FieldNameOf.recipeMaker);
            XElement recipeUsers = recipeMaker.Field(FieldNameOf.recipeUsers);
            if (recipeUsers != null) {
                StringBuilder users = new StringBuilder();
                users.Append(" Recipe Users: ");
                foreach (XElement user in recipeUsers.Elements()) {
                    users.Append(user.Value);
                    users.Append(", ");
                }
                users.Remove(users.Length - 2, 1);
                XComment lastComment = null;
                foreach (XNode node in root.Nodes()) {
                    if (node.NodeType == XmlNodeType.Comment)
                        lastComment = node as XComment;
                }
                if (users.ToString() != lastComment.Value) {
                    root.Add("\n  ", new XComment(users.ToString()), "\n\n");
                }
            }

            // <RecipeMake> and <RecipeMakeJobString> in KeyedMisc_Gameplay.xml
            string defNameValue = defName.Value;
            string labelValue = label.Value;
            root.Add("  ", new XElement("Make_" + defNameValue + ".label", string.Format("make {0}", labelValue)), "\n");
            root.Add("  ", new XElement("Make_" + defNameValue + ".description", string.Format("Make {0}.", labelValue)), "\n");
            root.Add("  ", new XElement("Make_" + defNameValue + ".jobString", string.Format("Making {0}.", labelValue)), "\n\n");
        }

        /// <summary>
        /// For administer recipes from drugs
        /// </summary>
        private void AddRecipesAddAdminister(XElement def, XElement defName) {
            XDocument doc = this.GetDoc(DefTypeNameOf.RecipeDef, "Recipes_Add_Administer.xml");
            XElement root = doc.Root;
            if (!root.HasElements) {
                root.Add("  ", new XComment(" SPECIAL: These Recipes from Drugs (Items with <ingestible> and <drugCategory>), generated by RimTrans "), "\n\n");
            }

            XElement label = def.label();
            if (label == null)
                label = defName;

            // <RecipeAdminister> and <RecipeAdministerJobString> in Keyed/Misc_Gameplay.xml
            string defNameValue = defName.Value;
            string labelValue = label.Value;
            root.Add("  ", new XElement("Administer_" + defNameValue + ".label", string.Format("administer {0}", labelValue)), "\n");
            root.Add("  ", new XElement("Administer_" + defNameValue + ".jobString", string.Format("Administering {0}.", labelValue)), "\n\n");
        }

        /// <summary>
        /// For natural stone floor
        /// </summary>
        private void AddTerrainAdd(XElement def, XElement defName) {
            XDocument doc = this.GetDoc(DefTypeNameOf.TerrainDef, "Terrain_Add.xml");
            XElement root = doc.Root;
            if (!root.HasElements) {
                root.Add("  ", new XComment(" SPECIAL: Terrains from natural stones "), "\n\n");
            }

            XElement label = def.label();
            if (label == null)
                label = defName;

            // Content generation in Keyed/Misc.xml
            string defNameValue = defName.Value;
            string labelValue = label.Value;
            root.Add("  ", new XElement(defNameValue + "_Rough.label", "rough " + labelValue), "\n");
            root.Add("  ", new XElement(defNameValue + "_Rough.description", string.Format("Rough, natural {0} ground.", labelValue)), "\n");
            root.Add("  ", new XElement(defNameValue + "_RoughHewn.label", "rough-hewn " + labelValue), "\n");
            root.Add("  ", new XElement(defNameValue + "_RoughHewn.description", string.Format("Roughly cut natural {0} floor.", labelValue)), "\n");
            root.Add("  ", new XElement(defNameValue + "_Smooth.label", "smooth " + labelValue), "\n");
            root.Add("  ", new XElement(defNameValue + "_Smooth.description", string.Format("Smoothed natural {0} floor.", labelValue)), "\n\n");
        }

        /// <summary>
        /// For blueprint and frame from buildable terrains.
        /// </summary>
        private void AddTerrainExtra(XElement def, XElement defName) {
            XDocument doc = this.GetDoc(DefTypeNameOf.ThingDef, "_Terrain_Extra.xml");
            XElement root = doc.Root;
            if (!root.HasElements) {
                root.Add("  ", new XComment(" SPECIAL: Blueprint and Frame of Terrian, generated by RimTrans "), "\n\n");
            }

            XElement label = def.label();
            if (label == null)
                label = defName;

            // Content generation in Keyed/Misc.xml
            string defNameValue = defName.Value;
            string labelValue = label.Value;
            root.Add("  ", new XElement(defNameValue + "_Blueprint.label", labelValue + " (blueprint)"), "\n");
            root.Add("  ", new XElement(defNameValue + "_Frame.label", labelValue + " (building)"), "\n");
            root.Add("  ", new XElement(defNameValue + "_Frame.description", "Terrain building in progress."), "\n\n");
        }

        private void AddKeyBindingCategoriesAddArchitect(XElement def, XElement defName) {
            XDocument doc = this.GetDocEx(DefTypeNameOf.KeyBindingCategoryDef, "KeyBindingCategories_Add_Architect.xml");
            XElement root = doc.Root;
            root.FirstAttribute.Value = "true";
            if (!root.HasElements) {
                root.Add("  ", new XComment(" SPECIAL: KeyBindingCategories for sections of the Architect menu, generated by RimTrans "), "\n\n");
            }

            XElement label = def.label();
            if (label == null)
                label = defName;

            root.Add("  ", new XElement("Architect_" + defName.Value + ".label", label.Value + " tab"), "\n");
            string labelCap = label.Value;
            labelCap = labelCap[0].ToString().ToUpper() + labelCap.Substring(1);
            root.Add("  ", new XElement("Architect_" + defName.Value + ".description", "Key bindings for the \"" + labelCap + "\" section of the Architect menu"), "\n\n");
        }

        /// <summary>
        /// For KeyBindingDef from MainTabDefs
        /// </summary>
        private void AddKeyBindingsAddMainTab(XElement def, XElement defName) {
            XDocument doc = this.GetDocEx(DefTypeNameOf.KeyBindingDef, "KeyBindings_Add_MainTab.xml");
            XElement root = doc.Root;
            root.FirstAttribute.Value = "true";
            if (!root.HasElements) {
                root.Add("  ", new XComment(" SPECIAL: KeyBindings for MainTabs, generated by RimTrans "), "\n\n");
            }

            XElement label = def.label();
            if (label == null)
                label = defName;

            root.Add("  ", new XElement("MainTab_" + defName.Value + ".label", "Toggle " + label.Value + " tab"), "\n");
        }

        #endregion

        #region Tidy

        /// <summary>
        /// Process: add newline at the end, remove duplicated nodes.
        /// </summary>
        private void Tidy() {
            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> defTypeNameSubDataPair in this._data) {
                string defTypeName = defTypeNameSubDataPair.Key;
                SortedDictionary<string, XDocument> subData = defTypeNameSubDataPair.Value;
                List<XElement> injections = new List<XElement>();
                foreach (XDocument doc in subData.Values) {
                    XElement root = doc.Root;
                    if (root.HasAttributes && root.FirstAttribute.Value == "true") {
                        root.Add("\n\n");
                    } else {
                        root.Add("\n");
                    }
                    root.RemoveAttributes();
                    injections.AddRange(root.Elements());
                }
                for (int i = 0; i < injections.Count; i++) {
                    XElement inject_i = injections[i];
                    for (int j = i + 1; j < injections.Count; j++) {
                        XElement inject_j = injections[j];
                        if (inject_i.Match(inject_j)) {
                            Log.Warning();
                            Log.Write("Duplicated node in DefInjected/{0}: ", defTypeName);
                            Log.WriteLine(ConsoleColor.Yellow, "<{0}>", inject_i.Name.ToString());
                            inject_i.ReplaceWith(new XComment("[Duplicated] " + inject_i.ToString()));
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region Match

        /// <summary>
        /// Process confict to Core DefInjected
        /// </summary>
        public void MatchCore(InjectionData injectionDataCore) {
            if (this._data.Count == 0)
                return;

            Log.Info();
            Log.WriteLine("Start checking conficts to Core's DefInjected.");
            List<XElement> conflicts = new List<XElement>();
            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> defTypNameSubDataPair in this._data) {
                SortedDictionary<string, XDocument> subData = defTypNameSubDataPair.Value;
                SortedDictionary<string, XDocument> subDataCore;
                if (injectionDataCore._data.TryGetValue(defTypNameSubDataPair.Key, out subDataCore)) {
                    IEnumerable<XElement> injections = from doc in subData.Values
                                                       from ele in doc.Root.Elements()
                                                       select ele;
                    IEnumerable<XElement> injectionsCore = from doc in subDataCore.Values
                                                           from ele in doc.Root.Elements()
                                                           select ele;
                    foreach (XElement inject in injections) {
                        foreach (XElement injectCore in injectionsCore) {
                            if (inject.Match(injectCore)) {
                                inject.Value = injectCore.Value;
                                conflicts.Add(inject);
                            }
                        }
                    }
                }
            }
            int countConflicts = conflicts.Count;
            if (countConflicts > 0) {
                foreach (XElement conf in conflicts) {
                    conf.ReplaceWith(new XComment("[Core] " + conf.ToString()));
                }
                Log.Info();
                Log.WriteLine("Completed processing DefInjected confict: {0} node(s)", countConflicts);
            } else {
                Log.Info();
                Log.WriteLine("No DefInjected confict to Core.");
            }
        }

        /// <summary>
        /// Continue to use existed DefInjected and comment invalid nodes
        /// </summary>
        public void MatchExisted(InjectionData injectionDataExisted) {
            if (injectionDataExisted._data.Count == 0)
                return;

            Log.Info();
            Log.WriteLine("Start matching existed DefInjected.");
            int countInvalidFiles = 0;
            int countMatched = 0;
            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> defTypeNameSubDataPair in injectionDataExisted._data) {
                SortedDictionary<string, XDocument> subDataExisted = defTypeNameSubDataPair.Value;
                SortedDictionary<string, XDocument> subData = null;
                string defTypeName = defTypeNameSubDataPair.Key;
                bool isNonStandard = false;
                this._data.TryGetValue(defTypeName, out subData);
                if (subData == null && defTypeName.Length > 3) {
                    this._data.TryGetValue(defTypeName.Substring(0, defTypeName.Length - 1), out subData);
                    isNonStandard = true;
                }
                if (subData == null) {
                    subData = new SortedDictionary<string, XDocument>();
                    foreach (KeyValuePair<string, XDocument> fileNameDocPair in subDataExisted) {
                        subData.Add(fileNameDocPair.Key, fileNameDocPair.Value.ToCommentDoc());
                        countInvalidFiles++;
                    }
                    this._data.Add(defTypeName, subData);
                } else {
                    IEnumerable<XElement> injectionsExisted = from doc in subDataExisted.Values
                                                              from ele in doc.Root.Elements()
                                                              select ele;
                    IEnumerable<XElement> injections = from doc in subData.Values
                                                       from ele in doc.Root.Elements()
                                                       select ele;
                    foreach (XElement inject in injections) {
                        foreach (XElement injectExsited in injectionsExisted) {
                            if (inject.Match(injectExsited)) {
                                inject.Value = injectExsited.Value;
                                countMatched++;
                            }
                        }
                    }
                    foreach (KeyValuePair<string, XDocument> fileNameDocPairExisted in subDataExisted) {
                        string fileName = fileNameDocPairExisted.Key;
                        XDocument docExisted = fileNameDocPairExisted.Value;
                        XDocument doc = null;
                        subData.TryGetValue(fileName, out doc);
                        if (doc == null) {
                            subData.Add(fileName, docExisted.ToCommentDoc());
                            countInvalidFiles++;
                        } else {
                            XElement rootExisted = docExisted.Root;
                            XElement root = doc.Root;
                            bool hasInvalidNodes = false;
                            foreach (XNode nodeExisted in rootExisted.Nodes()) {
                                if (nodeExisted.NodeType == XmlNodeType.Comment) {
                                    bool isMatched = false;
                                    foreach (XNode node in root.Nodes()) {
                                        if (node.NodeType == XmlNodeType.Comment &&
                                            ((XComment)nodeExisted).Value == ((XComment)node).Value) {
                                            isMatched = true;
                                            break;
                                        }
                                    }
                                    if (!isMatched) {
                                        root.Add("  ", nodeExisted, "\n");
                                        hasInvalidNodes = true;
                                    }
                                } else if (nodeExisted.NodeType == XmlNodeType.Element) {
                                    bool isMatched = false;
                                    foreach (XNode node in root.Nodes()) {
                                        if (node.NodeType == XmlNodeType.Element &&
                                            ((XElement)nodeExisted).Match(((XElement)node))) {
                                            isMatched = true;
                                            break;
                                        }
                                    }
                                    if (!isMatched) {
                                        root.Add("  ", new XComment(nodeExisted.ToString()), "\n");
                                        hasInvalidNodes = true;
                                    }
                                }
                            }
                            if (hasInvalidNodes)
                                root.Add("\n");
                        }
                    }
                    if (isNonStandard) {
                        subData = new SortedDictionary<string, XDocument>();
                        foreach (KeyValuePair<string, XDocument> fileNameDocPair in subDataExisted) {
                            subData.Add(fileNameDocPair.Key, fileNameDocPair.Value.ToCommentDoc());
                            countInvalidFiles++;
                        }
                        this._data.Add(defTypeName, subData);
                    }
                }
            }
            Log.Info();
            Log.WriteLine("Completed matching existed DefInjected: {0} matched node(s), {1} invalid file(s).", countMatched, countInvalidFiles);
        }

        #endregion

        #region Save

        /// <summary>
        /// Output this DefInjected to files.
        /// </summary>
        public void Save(string path) {
            if (this._data.Count() == 0)
                return;

            Log.Info();
            Log.Write("Start outputing DefInjected: ");
            Log.WriteLine(ConsoleColor.Cyan, path);

            if (Directory.Exists(path)) {
                DirectorySecurity ds = new DirectorySecurity(path, AccessControlSections.Access);
                if (ds.AreAccessRulesProtected) {
                    Log.Error();
                    Log.WriteLine("Outputing DefInjected failed: No write permission to directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    return;
                }
            } else {
                try {
                    Directory.CreateDirectory(path);
                } catch (Exception ex) {
                    Log.Error();
                    Log.WriteLine("Outputing DefInjected failed: Can not create directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    Log.Indent();
                    Log.WriteLine(ex.Message);
                    return;
                }
            }

            int countValidFiles = 0;
            int countInvalidFiles = 0;
            int countValidNodes = 0;
            int countInvalidNodes = 0;
            foreach (KeyValuePair<string, SortedDictionary<string, XDocument>> defTypNameSubDataPair in this._data) {
                string defTypeName = defTypNameSubDataPair.Key;
                SortedDictionary<string, XDocument> subData = defTypNameSubDataPair.Value;
                string subDirPath = Path.Combine(path, defTypeName);
                if (Directory.Exists(subDirPath)) {
                    DirectorySecurity curDs = new DirectorySecurity(subDirPath, AccessControlSections.Access);
                    if (curDs.AreAccessRulesProtected) {
                        Log.Error();
                        Log.WriteLine("Outputing to sub-directory failed: No write permission to directory.");
                        Log.Indent();
                        Log.WriteLine(ConsoleColor.Red, subDirPath);
                        countInvalidFiles += subData.Count;
                        IEnumerable<XElement> invalidNodes = from doc in subData.Values
                                                             from ele in doc.Root.Elements()
                                                             select ele;
                        countInvalidNodes += invalidNodes.Count();
                        continue;
                    }
                } else {
                    Directory.CreateDirectory(subDirPath);
                }
                foreach (KeyValuePair<string, XDocument> fileNameDocPair in subData) {
                    string filePath = Path.Combine(subDirPath, fileNameDocPair.Key);
                    XDocument doc = fileNameDocPair.Value;
                    XElement root = doc.Root;
                    if (defTypeName == DefTypeNameOf.InteractionDef ||
                        defTypeName == DefTypeNameOf.RulePackDef ||
                        defTypeName == DefTypeNameOf.TaleDef) {
                        // Special for these 3 DefType
                        try {
                            using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
                                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) // UTF-8 with BOM
                                {
                                    sw.WriteLine(doc.Declaration.ToString());
                                    sw.Write(root.ToString().Replace("-&gt;", "->"));
                                }
                            }
                            countValidFiles++;
                            countValidNodes += root.Elements().Count();
                        } catch (Exception ex) {
                            Log.Error();
                            Log.Write("Outputing file failed: ");
                            Log.WriteLine(ConsoleColor.Red, filePath);
                            Log.Indent();
                            Log.WriteLine(ex.Message);
                            countInvalidFiles++;
                            countInvalidNodes += root.Elements().Count();
                        }
                    } else {
                        // Universal
                        try {
                            fileNameDocPair.Value.Save(filePath);
                            countValidFiles++;
                            countValidNodes += root.Elements().Count();
                        } catch (Exception ex) {
                            Log.Error();
                            Log.Write("Outputing file failed: ");
                            Log.WriteLine(ConsoleColor.Red, filePath);
                            Log.Indent();
                            Log.WriteLine(ex.Message);
                            countInvalidFiles++;
                            countInvalidNodes += root.Elements().Count();
                        }
                    }
                }
            }
            if (countValidFiles > 0) {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine("Completed outputing DefInjected: {0} file(s), {1} node(s).", countValidFiles, countValidNodes);
                } else {
                    Log.Warning();
                    Log.WriteLine("Completed outputing DefInjected: Success: {0} file(s), {1} node(s); Failure {2} file(s), {3} node(s).",
                        countValidFiles, countValidNodes, countInvalidFiles, countInvalidNodes);
                }
            } else {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine("No DefInjected to be output.");
                } else {
                    Log.Error();
                    Log.WriteLine("Outputing DefInjected failed: {0} file(s), {1} node(s).", countInvalidFiles, countInvalidNodes);
                }
            }
        }

        #endregion

        #region Debug

        public void Debug(string defTypeName, string fileName) {
            SortedDictionary<string, XDocument> subData;
            if (this._data.TryGetValue(defTypeName, out subData)) {
                XDocument doc;
                if (subData.TryGetValue(fileName, out doc)) {
                    Log.Write(ConsoleColor.Cyan, defTypeName + "/" + fileName);
                    Log.WriteLine(doc.ToString());
                }
            }
        }

        public void Debug() {
            Log.WriteLine(ConsoleColor.Cyan, "InjectionData.Debug()");
            foreach (var defTypeNameSubDataPair in this._data) {
                Log.WriteLine(ConsoleColor.Cyan, defTypeNameSubDataPair.Key);
                foreach (var fileNameDocPair in defTypeNameSubDataPair.Value) {
                    Log.WriteLine(fileNameDocPair.Key);
                }
            }
            //Log.WriteLine("================");
            //Debug("RecipeDef", "Recipes_Add_Administer.xml");
            //Debug("RecipeDef", "Recipes_Add_Make.xml");
            //Debug("ThingDef", "_Terrain_Extra.xml");
            //Debug("ThingDef", "Buildings_Joy.xml");
            //Debug("TerrainDef", "Terrain_Add.xml");
            //Debug("ThingDef", "Races_Animal_Arid.xml");
            //Debug("PawnKindDef", "Races_Animal_Arid.xml");
        }

        #endregion
    }
}
