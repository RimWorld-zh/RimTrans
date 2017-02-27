using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RimTrans.Builder.Xml;

namespace RimTrans.Builder
{
    public class InjectionData
    {
        private string _path = string.Empty;
        public string Path { get { return this._path; } }

        private SortedDictionary<string, SortedDictionary<string, XDocument>> _data;

        private InjectionData()
        {

        }

        #region Load

        public static InjectionData Load(string path)
        {
            InjectionData injectionData = new InjectionData();
            injectionData._path = path;
            injectionData._data = new SortedDictionary<string, SortedDictionary<string, XDocument>>();
            
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists)
            {
                Log.Info();
                Log.Write("Loading DefInjected: ");
                Log.Cyan(path);
                Log.Line();
                int countValidFiles = 0;
                int countInvalidFiles = 0;
                foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
                {
                    SortedDictionary<string, XDocument> subData = new SortedDictionary<string, XDocument>();
                    int splitIndex = subDirInfo.FullName.Length + 1;
                    foreach (FileInfo fileInfo in subDirInfo.GetFiles("*.xml", SearchOption.AllDirectories))
                    {
                        XDocument doc = null;
                        try
                        {
                            doc = DocHelper.LoadLanguageDoc(fileInfo.FullName);
                            countValidFiles++;
                        }
                        catch (XmlException ex)
                        {
                            Log.Error();
                            Log.WriteLine(ex.Message);
                            Log.Indent();
                            Log.WriteLine(ex.SourceUri);
                            countInvalidFiles++;
                        }
                        if (doc != null)
                        {
                            subData.Add(fileInfo.FullName.Substring(splitIndex), doc);
                        }
                    }
                    if (subData.Count() > 0)
                    {
                        injectionData._data.Add(subDirInfo.Name, subData);
                    }
                }
                if (countValidFiles > 0)
                {
                    Log.Info();
                    Log.WriteLine("Completed Loading DefInjected: Success: {0} file(s), Failure: {1} file(s).", countValidFiles, countInvalidFiles);
                }
                else if (countInvalidFiles > 0)
                {
                    Log.Error();
                    Log.WriteLine("Loading failed: {1} file(s).", countInvalidFiles);
                }
                else
                {
                    Log.Info();
                    Log.WriteLine("Directory \"DefInjected\" is empty.");
                }
            }
            else
            {
                Log.Info();
                Log.Write("Directory \"DefInjected\" does not exist: ");
                Log.Cyan(path);
                Log.Line();
            }
            return injectionData;
        }

        #endregion

        #region Parse

        public static InjectionData Parse(DefinitionData definitionData, bool isCore = false)
        {
            InjectionData injectionData = new InjectionData();
            injectionData._data = new SortedDictionary<string, SortedDictionary<string, XDocument>>();

            SortedDictionary<string, XDocument> definitions = definitionData.Data;
            if (definitions.Count() > 0)
            {
                Log.Info();
                Log.Write("Start parsing Defs and generating ");
                Log.Cyan("Original DefInjected");
                Log.WriteLine(".");
                int countValidDefs = 0;
                int countInvalidDefs = 0;
                int countFields = 0;
                foreach (KeyValuePair<string, XDocument> pathDocPair in definitions)
                {
                    string commentText = null;
                    foreach (XNode node in pathDocPair.Value.Root.Nodes())
                    {
                        if (node.NodeType == XmlNodeType.Comment)
                        {
                            XComment curComment = node as XComment;
                            string text = curComment.Value.Trim().Trim('=').Trim();
                            if (text.Length > 0)
                            {
                                commentText = text;
                            }
                        }
                        else if (node.NodeType == XmlNodeType.Element && (node as XElement).IsNeedToTranslate())
                        {
                            XElement def = node as XElement;
                            XElement defName = def.defName();
                            if (defName != null)
                            {
                                bool isValid = false;
                                try
                                {
                                    XElement tempEle = new XElement(defName.Value);
                                    isValid = true;
                                }
                                catch (XmlException ex)
                                {
                                    Log.Error();
                                    Log.WriteLine("Invalid <defName>{0}</defName>", defName.Value);
                                    Log.Indent();
                                    Log.WriteLine(ex.Message);
                                    Log.Indent();
                                    Log.WriteLine(defName.BaseUri);
                                    countInvalidDefs++;
                                }
                                if (isValid)
                                {
                                    int count = injectionData.AddFromDef(def, pathDocPair, defName, commentText);
                                    if (count > 0)
                                    {
                                        countValidDefs++;
                                        countFields += count;
                                    }
                                }
                            }
                        }
                    }
                }
                if (countFields > 0)
                {
                    injectionData.Tidy();
                    if (isCore)
                    {
                        // Something for core
                    }
                    if (countInvalidDefs > 0)
                    {
                        Log.Error();
                        Log.WriteLine("Encountered {0} invalid Defs during parsing. Other Defs have been parsed successfully", countInvalidDefs);
                    }
                    Log.Info();
                    Log.WriteLine("Completed generating DefInjected: {0} Def(s) -> {1} DefInjected field(s).", countValidDefs, countFields);
                }
                else if (countInvalidDefs > 0)
                {
                    Log.Error();
                    Log.WriteLine("Generating DefInjected failed: {0} invalid Def(s).", countInvalidDefs);
                }
                else
                {
                    Log.Info();
                    Log.WriteLine("Completed parsing and no DefInjected to be generated.");
                }
            }

            return injectionData;
        }

        #endregion

        #region Get Doc

        private XDocument GetDoc(string defTypeName, string fileName)
        {
            XDocument doc;
            SortedDictionary<string, XDocument> subData;
            if (this._data.TryGetValue(defTypeName, out subData))
            {
                if (!subData.TryGetValue(fileName, out doc))
                {
                    doc = DocHelper.EmptyDoc();
                    subData.Add(fileName, doc);
                }
            }
            else
            {
                doc = DocHelper.EmptyDoc();
                subData = new SortedDictionary<string, XDocument>();
                subData.Add(fileName, doc);
                this._data.Add(defTypeName, subData);
            }
            return doc;
        }

        private XDocument GetDocEx(string defTypeName, string fileName)
        {
            XDocument doc;
            SortedDictionary<string, XDocument> subData;
            if (this._data.TryGetValue(defTypeName, out subData))
            {
                if (!subData.TryGetValue(fileName, out doc))
                {
                    doc = DocHelper.EmptyDocEx();
                    subData.Add(fileName, doc);
                }
            }
            else
            {
                doc = DocHelper.EmptyDocEx();
                subData = new SortedDictionary<string, XDocument>();
                subData.Add(fileName, doc);
                this._data.Add(defTypeName, subData);
            }
            return doc;
        }

        #endregion

        #region Generate and Add

        private int AddFromDef(XElement def, KeyValuePair<string, XDocument> pathDocPair, XElement defName, string commentText)
        {
            int result = 0;

            bool isMote = false;
            bool isBuildable = false;
            bool isMinifiable = false;
            bool isMakeable = false;
            bool isDrug = false;
            bool isPawn = false;

            string defTypeName = def.Name.ToString();
            if (defTypeName == DefTypeNameOf.ThingDef)
            {
                XElement category = def.Field(FieldNameOf.category);
                if (category != null)
                {
                    string thingCategory = category.Value;
                    if (thingCategory == ThingCategoryOf.Mote)
                    {
                        isMote = true;
                    }
                    else if (thingCategory == ThingCategoryOf.Building)
                    {
                        if (def.Field(FieldNameOf.designationCategory) != null) isBuildable = true;
                        if (def.Field(FieldNameOf.minifiedDef) != null) isMinifiable = true;
                        if (def.Field(FieldNameOf.recipeMaker) != null) isMakeable = true;
                        XElement building = def.Field(FieldNameOf.building);
                        if (building != null)
                        {
                            XElement isNaturalRock = building.Field(FieldNameOf.isNaturalRock);
                            XElement isResourceRock = building.Field(FieldNameOf.isResourceRock);
                            if (isNaturalRock != null  && string.Compare(isNaturalRock.Value, "true", true) == 0 &&
                                (isResourceRock == null || string.Compare(isResourceRock.Value, "false", true) == 0))
                            {
                                this.AddTerrainAdd(def, defName);
                                result += 6;
                            }
                        }
                    }
                    else if (thingCategory == ThingCategoryOf.Item)
                    {
                        if (def.Field(FieldNameOf.recipeMaker) != null) isMakeable = true;
                        XElement ingestible = def.Field(FieldNameOf.ingestible);
                        if (ingestible != null)
                        {
                            XElement drugCategory = ingestible.Field(FieldNameOf.drugCategory);
                            if (drugCategory != null &&
                                drugCategory.Value != "None")
                            {
                                isDrug = true;
                            }
                        }
                    }
                    else if (thingCategory == ThingCategoryOf.Pawn)
                    {
                        isPawn = true;
                    }
                }
            }
            else if (defTypeName == DefTypeNameOf.TerrainDef && def.Field(FieldNameOf.designationCategory) != null)
            {
                this.AddTerrainExtra(def, defName);
                result += 2;
            }

            if (!isMote)
            {
                LinkedList<XElement> fieldPath = new LinkedList<XElement>();
                fieldPath.AddLast(defName);
                IEnumerable<object> contents = GenRecursively(def, fieldPath);
                if (isBuildable || isMinifiable)
                {
                    contents = contents.Concat(GenExtraBuilding(def, defName, isBuildable, isMinifiable));
                }
                if (isPawn)
                {
                    contents = contents.Concat(GenExtraPawn(def, defName));
                }
                int countFields = 0;
                foreach (object item in contents)
                {
                    if (item is XElement) countFields++;
                }
                if (countFields > 0)
                {
                    XDocument doc = this.GetDocEx(def.Name.ToString(), System.IO.Path.GetFileName(pathDocPair.Key));
                    XElement root = doc.Root;
                    XComment lastComment = null;
                    foreach (XNode node in root.Nodes())
                    {
                        if (node.NodeType == XmlNodeType.Comment) lastComment = node as XComment;
                    }
                    XAttribute layout = root.FirstAttribute;
                    if (commentText != null &&
                        (lastComment == null || lastComment.Value != commentText))
                    {
                        if (layout.Value == "true")
                        {
                            layout.Value = "false";
                            if (root.HasElements)
                            {
                                root.Add("\n\n");
                            }
                            else
                            {
                                root.Add("\n");
                            }
                            root.Add("  ", new XComment(commentText), "\n\n");
                        }
                    }
                    if (countFields == 1)
                    {
                        layout.Value = "true";
                        root.Add(contents);
                    }
                    else
                    {
                        if (layout.Value == "true")
                        {
                            layout.Value = "false";
                            root.Add("\n");
                        }
                        root.Add(contents);
                        root.Add("\n");
                    }
                }
                if (isMakeable)
                {
                    this.AddRecipesAddMake(def, defName);
                    result += 3;
                }
                if (isDrug)
                {
                    this.AddRecipesAddAdminister(def, defName);
                    result += 2;
                }
                result += countFields;
            }
            return result;
        }

        private static IEnumerable<object> GenRecursively(XElement field, LinkedList<XElement> fieldPath)
        {
            foreach (XElement fieldChild in field.Elements())
            {
                if (fieldChild.HasElements)
                {
                    fieldPath.AddLast(fieldChild);
                    foreach (object content in GenRecursively(fieldChild, fieldPath))
                    {
                        yield return content;
                    }
                    fieldPath.RemoveLast();
                }
                else if (fieldChild.IsInjectable() || fieldChild.IsInjectableExtra())
                {
                    StringBuilder fullFieldName = new StringBuilder();
                    bool isDefName = true;
                    foreach (XElement linkedField in fieldPath)
                    {
                        if (isDefName)
                        {
                            fullFieldName.Append(linkedField.Value);
                            isDefName = false;
                        }
                        else if (linkedField.Name == "li")
                        {
                            fullFieldName.Append(linkedField.Attribute("ListIndex").Value);
                        }
                        else
                        {
                            fullFieldName.Append(linkedField.Name.ToString());
                        }
                        fullFieldName.Append('.');
                    }
                    fullFieldName.Append(fieldChild.Name.ToString());
                    yield return "  ";
                    yield return new XElement(fullFieldName.ToString(), fieldChild.Value);
                    yield return "\n";
                }
            }
        }

        private static IEnumerable<object> GenExtraBuilding(XElement def, XElement defName, bool isBuildable, bool isMinifiable)
        {
            XElement label = def.label();
            if (label == null) label = defName;
            if (isBuildable)
            {
                yield return "  ";
                yield return new XElement(defName.Value + "_Blueprint.label", label.Value + " (blueprint)");
                yield return "\n";
            }
            if (isMinifiable)
            {
                yield return "  ";
                yield return new XElement(defName.Value + "_Blueprint_Install.label", label.Value + " (blueprint)");
                yield return "\n";
            }
            if (isBuildable)
            {
                yield return "  ";
                yield return new XElement(defName.Value + "_Frame.label", label.Value + " (building)");
                yield return "\n";
            }
        }

        private static IEnumerable<object> GenExtraPawn(XElement def, XElement defName)
        {
            XElement label = def.label();
            if (label == null) label = defName;
            string defNameValue = defName.Value;
            string labelValue = label.Value;

            XElement butcherProducts = def.Field(FieldNameOf.butcherProducts);
            XElement LeatherAmount = null;
            XElement MeatAmount = null;
            XElement statBases = def.Field(FieldNameOf.statBases);
            if (statBases != null)
            {
                LeatherAmount = statBases.Element("LeatherAmount");
                MeatAmount = statBases.Element("MeatAmount");
            }
            XElement fleshType = null;
            XElement useLeatherFrom = null;
            XElement useMeatFrom = null;
            XElement leatherLabel = null;
            XElement meatLabel = null;
            XElement race = def.Field(FieldNameOf.race);
            if (race != null)
            {
                fleshType = race.Field(FieldNameOf.fleshType);
                useLeatherFrom = race.Field(FieldNameOf.useLeatherFrom);
                useMeatFrom = race.Field(FieldNameOf.useMeatFrom);
                leatherLabel = race.Field(FieldNameOf.leatherLabel);
                meatLabel = race.Field(FieldNameOf.meatLabel);
            }

            if (fleshType != null && fleshType.Value == "Mechanoid")
            {
                yield return "  ";
                yield return new XComment(" Flesh Type: Mechanoid ");
                yield return "\n";
            }
            else
            {
                // Leather
                if (butcherProducts != null)
                {
                    yield return "  ";
                    yield return new XComment(" No Leather: Butcher Products ");
                    yield return "\n";
                }
                else if (LeatherAmount != null && LeatherAmount.Value == "0")
                {
                    yield return "  ";
                    yield return new XComment(" Leather Amount: 0 ");
                    yield return "\n";
                }
                else if (useLeatherFrom != null)
                {
                    yield return "  ";
                    yield return new XComment(string.Format(" Use Leather From: {0} ", useLeatherFrom.Value));
                    yield return "\n";
                }
                else
                {
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
                if (MeatAmount != null && MeatAmount.Value == "0")
                {
                    yield return "  ";
                    yield return new XComment(" Meat Amount: 0 ");
                    yield return "\n";
                }
                else if (useMeatFrom != null)
                {
                    yield return "  ";
                    yield return new XComment(string.Format(" Use Meat From: {0} ", useMeatFrom.Value));
                    yield return "\n";
                }
                else
                {
                    string meatLabelValue =
                        meatLabel == null ?
                        string.Format("{0} meat", labelValue) :
                        meatLabel.Value;
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

        private void AddRecipesAddMake(XElement def, XElement defName)
        {
            XDocument doc = this.GetDoc(DefTypeNameOf.RecipeDef, "Recipes_Add_Make.xml");
            XElement root = doc.Root;
            if (root.Elements().Count() == 0)
            {
                root.Add("  ", new XComment(" SPECIAL: These Recipes from makeable Items and Buildings (ThingDef with <recipeMaker>), generated by RimTrans "), "\n\n");
            }

            XElement label = def.label();
            if (label == null) label = defName;

            XElement recipeMaker = def.Field(FieldNameOf.recipeMaker);
            XElement recipeUsers = recipeMaker.Field(FieldNameOf.recipeUsers);
            if (recipeUsers != null)
            {
                StringBuilder users = new StringBuilder();
                users.Append(" Recipe Users: ");
                foreach (XElement user in recipeUsers.Elements())
                {
                    users.Append(user.Value);
                    users.Append(", ");
                }
                users.Remove(users.Length - 2, 1);
                XComment lastComment = null;
                foreach (XNode node in root.Nodes())
                {
                    if (node.NodeType == XmlNodeType.Comment) lastComment = node as XComment;
                }
                if (users.ToString() != lastComment.Value)
                {
                    root.Add("\n  ", new XComment(users.ToString()), "\n\n");
                }
            }

            // RecipeMake and RecipeMakeJobString in Misc_Gameplay.xml
            string defNameValue = defName.Value;
            string labelValue = label.Value;
            root.Add("  ", new XElement("Make_" + defNameValue + ".label", string.Format("Make {0}", labelValue)), "\n");
            root.Add("  ", new XElement("Make_" + defNameValue + ".description", string.Format("Make {0}.", labelValue)), "\n");
            root.Add("  ", new XElement("Make_" + defNameValue + ".jobString", string.Format("Making {0}.", labelValue)), "\n\n");
        }

        private void AddRecipesAddAdminister(XElement def, XElement defName)
        {
            XDocument doc = this.GetDoc(DefTypeNameOf.RecipeDef, "Recipes_Add_Administer.xml");
            XElement root = doc.Root;
            if (root.Elements().Count() == 0)
            {
                root.Add("  ", new XComment(" SPECIAL: These Recipes from Drugs (Items with <ingestible> and <drugCategory>), generated by RimTrans "), "\n\n");
            }

            XElement label = def.label();
            if (label == null) label = defName;

            // RecipeAdminister and RecipeAdministerJobString in Misc_Gameplay.xml
            string defNameValue = defName.Value;
            string labelValue = label.Value;
            root.Add("  ", new XElement("Administer_" + defNameValue + ".label", string.Format("Administer {0}", labelValue)), "\n");
            root.Add("  ", new XElement("Administer_" + defNameValue + ".jobString", string.Format("Administering {0}.", labelValue)), "\n\n");
        }

        /// <summary>
        /// For vanilla natural stone floor
        /// </summary>
        private void AddTerrainAdd(XElement def, XElement defName)
        {
            XDocument doc = this.GetDoc(DefTypeNameOf.TerrainDef, "Terrain_Add.xml");
            XElement root = doc.Root;
            if (root.Elements().Count() == 0)
            {
                root.Add("  ", new XComment(" SPECIAL: Floors from natural stones "), "\n\n");
            }

            XElement label = def.label();
            if (label == null) label = defName;

            string defNameValue = defName.Value;
            string labelValue = label.Value;
            root.Add("  ", new XElement(defNameValue + "_Rough.label", "rough " + labelValue), "\n");
            root.Add("  ", new XElement(defNameValue + "_Rough.description", string.Format("Rough, natural {0} ground.", labelValue)), "\n");
            root.Add("  ", new XElement(defNameValue + "_RoughHewn.label", "rough-hewn " + labelValue), "\n");
            root.Add("  ", new XElement(defNameValue + "_RoughHewn.description", string.Format("Roughly cut natural {0} floor.", labelValue)), "\n");
            root.Add("  ", new XElement(defNameValue + "_Smooth.label", "smooth " + labelValue), "\n");
            root.Add("  ", new XElement(defNameValue + "_Smooth.description", string.Format("Smoothed natural {0} floor.", labelValue)), "\n\n");
        }

        private void AddTerrainExtra(XElement def, XElement defName)
        {
            XDocument doc = this.GetDoc(DefTypeNameOf.ThingDef, "_Terrain_Extra.xml");
            XElement root = doc.Root;
            if (root.Elements().Count() == 0)
            {
                root.Add("  ", new XComment(" SPECIAL: Blueprint and Frame of Terrian, generated by RimTrans "), "\n\n");
            }

            XElement label = def.label();
            if (label == null) label = defName;

            string defNameValue = defName.Value;
            string labelValue = label.Value;
            root.Add("  ", new XElement(defNameValue + "_Blueprint.label", labelValue + " (blueprint)"), "\n");
            root.Add("  ", new XElement(defNameValue + "_Frame.label", labelValue + " (building)"), "\n\n");
        }

        private void AddKeyBindingsAddMainTab(XElement def, XElement defName)
        {
            XDocument doc = this.GetDocEx(DefTypeNameOf.KeyBindingDef, "KeyBindings_Add_MainTab.xml");
            XElement root = doc.Root;
            if (root.Elements().Count() == 0)
            {
                root.Add("  ", new XComment(" SPECIAL: KeyBinding of MainTab, generated by RimTrans "), "\n\n");
            }

            XElement label = def.label();
            if (label == null) label = defName;

            root.Add("  ", new XElement("MainTab_" + defName.Value, "Toggle " + label.Value + " tab"), "\n");
        }

        #endregion

        #region Tidy

        private void Tidy()
        {
            foreach (SortedDictionary<string, XDocument> subData in this._data.Values)
            {
                foreach (XDocument doc in subData.Values)
                {
                    XElement root = doc.Root;
                    if (root.HasAttributes && root.FirstAttribute.Value == "true")
                    {
                        root.Add("\n\n");
                    }
                    else
                    {
                        root.Add("\n");
                    }
                    root.RemoveAttributes();
                }
            }
        }

        #endregion

        #region Debug

        public void Debug(string defTypeName, string fileName)
        {
            SortedDictionary<string, XDocument> subData;
            if (this._data.TryGetValue(defTypeName, out subData))
            {
                XDocument doc;
                if (subData.TryGetValue(fileName, out doc))
                {
                    Log.Cyan(defTypeName + "/" + fileName);
                    Log.WriteLine(doc.ToString());
                }
            }
        }

        public void Debug()
        {
            foreach (var kvpSubData in this._data)
            {
                Log.Cyan(kvpSubData.Key);
                Log.Line();
                foreach (var kvp in kvpSubData.Value)
                {
                    Log.WriteLine(kvp.Key);
                }
            }
            //Log.WriteLine("================");
            //Debug("RecipeDef", "Recipes_Add_Administer.xml");
            //Debug("RecipeDef", "Recipes_Add_Make.xml");
            //Debug("ThingDef", "_Terrain_Extra.xml");
            //Debug("ThingDef", "Buildings_Joy.xml");
            Debug("TerrainDef", "Terrain_Add.xml");
            //Debug("ThingDef", "Races_Animal_Arid.xml");
            //Debug("PawnKindDef", "Races_Animal_Arid.xml");
        }

        #endregion
    }
}
