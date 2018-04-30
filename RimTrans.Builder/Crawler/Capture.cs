using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using RimTrans.Builder.Xml;

namespace RimTrans.Builder.Crawler {
    public class Capture {
        /// <summary>
        /// If true, distinguish ThingDef category.
        /// </summary>
        public bool IsTemplate { get { return this.isTemplate; } }
        private readonly bool isTemplate;

        /// <summary>
        /// Get all Defs of this capture.
        /// </summary>
        public IEnumerable<DefInfo> Defs { get { return this.allDefInfo.Values; } }
        private SortedDictionary<string, DefInfo> allDefInfo;


        #region Methods

        public DefInfo Def(XElement defElement) {
            DefInfo defInfo;
            string defTypeName = defElement.Name.ToString();
            if (this.isTemplate && defTypeName == DefTypeNameOf.ThingDef) {
                XElement category = defElement.Field(FieldNameOf.category);
                if (category != null) {
                    defTypeName = $"{defTypeName}_{category.Value}";
                }
            }
            if (this.allDefInfo.TryGetValue(defTypeName, out defInfo)) {
                return defInfo;
            } else {
                return null;
            }
        }

        #endregion


        /// <summary>
        /// Initialize a capture.
        /// </summary>
        /// <param name="isTemplate">If true, distinguish ThingDef category.</param>
        private Capture(bool isTemplate) {
            this.isTemplate = isTemplate;
            this.allDefInfo = new SortedDictionary<string, DefInfo>();
        }

        private Capture() {
            this.allDefInfo = new SortedDictionary<string, DefInfo>();
        }

        /// <summary>
        /// Create capture from Core definition data.
        /// </summary>
        /// <param name="coreDefinitionData"></param>
        /// <param name="sourceCodePath">The path of source code folder.</param>
        /// <param name="isTemplate">If true, distinguish ThingDef category.</param>
        /// <returns></returns>
        public static Capture Parse(DefinitionData coreDefinitionData, string sourceCodePath = null, bool isTemplate = false) {
            Helper.sourceCodePath = sourceCodePath;
            Capture capture = new Capture(isTemplate);

            Log.Info();
            Log.Write("Start parsing Core Defs and generating ");
            if (isTemplate) {
                Log.Write(ConsoleColor.Cyan, "Templates");
            } else {
                Log.Write(ConsoleColor.Cyan, "Capture");
            }
            Log.WriteLine(".");

            SortedDictionary<string, XElement> allUniqueDefElements = new SortedDictionary<string, XElement>();

            foreach (XDocument doc in coreDefinitionData.Data.Values) {
                foreach (XElement current in doc.Root.Elements()) {
                    XAttribute isAbstract = current.Attribute("Abstract");
                    if (isAbstract != null && string.Compare(isAbstract.Value, "True", true) == 0)
                        continue;

                    string key = current.Name.ToString();
                    if (isTemplate && key == DefTypeNameOf.ThingDef) {
                        XElement thingClass = current.Field(FieldNameOf.category);
                        if (thingClass == null)
                            key = $"{key}_None";
                        else
                            key = $"{key}_{thingClass.Value}";
                    }
                    XElement def;
                    if (allUniqueDefElements.TryGetValue(key, out def)) {
                        Helper.MergeElement(def, current);
                    } else {
                        allUniqueDefElements.Add(key, new XElement(current));
                        continue;
                    }
                }
            }

            foreach (XElement curDef in allUniqueDefElements.Values) {
                Helper.MergeListItems(curDef);
            }

            foreach (XElement curDef in allUniqueDefElements.Values) {
                curDef.RemoveAttributes();
                string key = curDef.Name.ToString();
                if (isTemplate && key == DefTypeNameOf.ThingDef) {
                    XElement category = curDef.Field(FieldNameOf.category);
                    if (category != null)
                        key = $"{key}_{category.Value}";
                }
                capture.allDefInfo.Add(key, new DefInfo(curDef));
            }

            Log.Info();
            if (isTemplate) {
                Log.WriteLine($"Completed generating Templates: {capture.allDefInfo.Count} templates.");
            } else {
                Log.WriteLine($"Completed generating Capture: {capture.allDefInfo.Count} Def types.");
            }

            return capture;
        }


        #region ToXDocuments and Save

        /// <summary>
        /// Get XDocument of this cpature.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, XDocument> ToXDocuments() {
            Dictionary<string, XDocument> dict = new Dictionary<string, XDocument>();
            foreach (var kvp in this.allDefInfo) {

                XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null), new XElement("Defs"));
                XElement defElement = kvp.Value.ToXElement();
                doc.Root.Add(defElement);
                if (this.isTemplate && defElement.Name.ToString() == DefTypeNameOf.ThingDef) {
                    XElement category = defElement.Field(FieldNameOf.category);
                    if (category != null)
                        category.Value = kvp.Key.Substring(DefTypeNameOf.ThingDef.Length + 1);
                }
                dict.Add(kvp.Key, doc);
            }
            return dict;
        }

        /// <summary>
        /// Save this capture as xml files to a diretory. All the existed files will be deleted.
        /// </summary>
        /// <param name="path">The output directory.</param>
        public void Save(string path) {
            if (this.allDefInfo == null || this.allDefInfo.Count == 0)
                return;

            if (Directory.Exists(path)) {
                DirectorySecurity ds = new DirectorySecurity(path, AccessControlSections.Access);
                if (ds.AreAccessRulesProtected) {
                    Log.Error();
                    Log.WriteLine("Outputing Templates failure: No write permission to directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    return;
                } else {
                    DirectoryHelper.CleanDirectory(path, "*.xml");
                }
            } else {
                try {
                    Directory.CreateDirectory(path);
                } catch (Exception ex) {
                    Log.Error();
                    Log.WriteLine("Outputing Templates failure: Can not create directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    Log.Indent();
                    Log.WriteLine(ex.Message);
                    return;
                }
            }

            Log.Info();
            Log.Write("Start outputing Templates: ");
            Log.WriteLine(ConsoleColor.Cyan, path);

            Dictionary<string, XDocument> docsDict = this.ToXDocuments();
            int countValidFiles = 0;
            int countInvalidFiles = 0;

            foreach (KeyValuePair<string, XDocument> fileNameDoc in docsDict) {
                string filePath = Path.Combine(path, fileNameDoc.Key) + ".xml";
                try {
                    fileNameDoc.Value.Save(filePath);
                    countValidFiles++;
                } catch (Exception ex) {
                    Log.Error();
                    Log.Write("Outputing file failed: ");
                    Log.WriteLine(ConsoleColor.Red, filePath);
                    Log.Indent();
                    Log.WriteLine(ex.Message);
                    countInvalidFiles++;
                }
            }
            if (countValidFiles > 0) {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine($"Completed outputing Templates: {countValidFiles} file(s).");
                } else {
                    Log.Warning();
                    Log.WriteLine($"Completed outputing Templates: Success: {countValidFiles} file(s), Failure {countInvalidFiles} file(s).");
                }
            } else {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine("No Templates to be output.");
                } else {
                    Log.Error();
                    Log.WriteLine($"Outputing Templates failure: {countInvalidFiles} file(s).");
                }
            }
        }

        #endregion


        #region ProcessFieldNames

        /// <summary>
        /// Process case and alias of all fields in this definition data.
        /// </summary>
        /// <param name="definitionData"></param>
        public void ProcessFieldNames(DefinitionData definitionData) {
            if (this.isTemplate)
                return;

            Log.Info();
            Log.WriteLine("Start processing field names for all Defs.");

            definitionData.MarkProcessedFieldNames();

            foreach (XElement curDef in from doc in definitionData.Data.Values
                                        from ele in doc.Root.Elements()
                                        select ele) {
                string curDefTypeName = curDef.Name.ToString();
                DefInfo defInfo;
                if (this.allDefInfo.TryGetValue(curDefTypeName, out defInfo)) {
                    defInfo.ProcessFieldNames(curDef);
                } else {
                    //Log.Warning();
                    //Log.WriteLine($"'{curDefTypeName}' no matched.");
                }
            }

            Log.Info();
            Log.WriteLine("Completed processing field names for all Defs.");
        }

        #endregion




        public void Debug() {

        }
    }
}
