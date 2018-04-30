using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using RimTrans.Builder.Xml;
using RimTrans.Builder.Crawler;

namespace RimTrans.Builder.Wiki {
    /// <summary>
    /// For craating wiki data.
    /// </summary>
    public class WikiData {
        private static readonly UTF8Encoding utf8 = new UTF8Encoding(false);

        private SortedDictionary<string, SortedDictionary<string, SetDict>> allSetDict;

        private WikiData() {
            this.allSetDict = new SortedDictionary<string, SortedDictionary<string, SetDict>>();
        }

        /// <summary>
        /// Create wiki data from definition data and template capture.
        /// </summary>
        /// <param name="coreDefinitionData"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static WikiData Parse(DefinitionData coreDefinitionData, Capture template, params InjectionData[] injectionDatas) {
            if (!coreDefinitionData.IsProcessedFieldNames)
                throw new ArgumentException("Non-processed definition data.", "definitionData");

            if (!template.IsTemplate)
                throw new ArgumentException("Used non-template capture", "template");

            Log.Info();
            Log.Write("Start parsing Core Defs and Template Capture to generating ");
            Log.Write(ConsoleColor.Cyan, "Wiki Data");
            Log.WriteLine(".");

            WikiData wikiData = new WikiData();

            int countSetDicts = 0;

            coreDefinitionData = new DefinitionData(coreDefinitionData);

            Dictionary<string, List<XElement>> unnamedDefsDict = new Dictionary<string, List<XElement>>();
            foreach (XElement curDefElement in from doc in coreDefinitionData.Data.Values
                                               from ele in doc.Root.Elements()
                                               where ele.Attribute("Abstract") == null
                                               select ele) {
                string curDefType = curDefElement.Name.ToString();
                XElement defName = curDefElement.Element("defName");
                string curPageName;
                if (defName == null) {
                    int count_curUnnamedDefs;
                    List<XElement> curUnnamedDefsList;
                    if (unnamedDefsDict.TryGetValue(curDefType, out curUnnamedDefsList)) {
                        count_curUnnamedDefs = curUnnamedDefsList.Count;
                        curUnnamedDefsList.Add(curDefElement);
                    } else {
                        count_curUnnamedDefs = 0;
                        curUnnamedDefsList = new List<XElement>();
                        curUnnamedDefsList.Add(curDefElement);
                        unnamedDefsDict.Add(curDefType, curUnnamedDefsList);
                    }
                    curPageName = $"Defs_{curDefType}_UnnamedDef_{count_curUnnamedDefs}";
                } else {
                    curPageName = $"Defs_{curDefType}_{defName.Value}";
                }

                Inject(curDefElement, injectionDatas);
                DefInfo curDefInfo = template.Def(curDefElement);
                if (curDefInfo == null) {
                    Log.Warning();
                    Log.WriteLine($"'{curPageName}' no matched DefInfo.");
                    continue;
                }
                if (!curDefInfo.IsValid) {
                    Log.Warning();
                    Log.WriteLine($"'{curPageName}' matched invalid DefInfo.");
                    continue;
                }
                SetDict curSetDict = new SetDict(curDefElement, curDefInfo);

                SortedDictionary<string, SetDict> curSubDict;
                if (wikiData.allSetDict.TryGetValue(curDefType, out curSubDict)) {
                    if (curSubDict.ContainsKey(curPageName)) {
                        Log.Warning();
                        Log.WriteLine($"Duplicated SetDict '{curPageName}'.");
                        curSubDict[curPageName] = curSetDict;
                    } else {
                        curSubDict.Add(curPageName, curSetDict);
                    }
                } else {
                    curSubDict = new SortedDictionary<string, SetDict>();
                    curSubDict.Add(curPageName, curSetDict);
                    wikiData.allSetDict.Add(curDefType, curSubDict);
                }
                countSetDicts++;
            }

            Log.Info();
            Log.WriteLine($"Complete generating wiki data: {wikiData.allSetDict.Count} DefTypes, {countSetDicts} SetDicts.");

            return wikiData;
        }

        #region Inject

        private static void Inject(XElement defElement, params InjectionData[] injectionDatas) {
            XElement defNameElement = defElement.defName();

            if (defNameElement == null)
                return;

            string defType = defElement.Name.ToString();
            string defName = defNameElement.Value;

            foreach (InjectionData curInjectionData in injectionDatas) {
                string langCode = curInjectionData.Code;
                IEnumerable<XElement> injections = curInjectionData.Injections(defType, defName);
                foreach (XElement curInjection in injections) {
                    XElement parent = defElement;
                    string[] pathSections = curInjection.Name.ToString().Split(new char[] { '.' });
                    for (int i = 1; i < pathSections.Length - 1; i++) {
                        XElement tempElement = null;
                        string tempElementName = pathSections[i];
                        int tempListIndex;
                        if (int.TryParse(tempElementName, out tempListIndex)) {
                            foreach (XElement curItemElement in parent.Elements()) {
                                if (curItemElement.Attribute("ListIndex").Value == tempElementName) {
                                    tempElement = curItemElement;
                                    break;
                                }
                            }
                        } else {
                            tempElement = parent.Element(tempElementName);
                        }
                        if (tempElement == null) {
                            parent.Add(new XElement(pathSections[1]));
                            parent = parent.Element(pathSections[1]);
                        } else {
                            parent = tempElement;
                        }
                    }
                    XElement newChild;
                    string newChildName = pathSections[pathSections.Length - 1];
                    int newChildListIndex;
                    if (int.TryParse(newChildName, out newChildListIndex)) {
                        newChild = new XElement("li", curInjection.Value);
                        newChild.SetAttributeValue("ListIndex", newChildListIndex);
                    } else {
                        newChild = new XElement(newChildName, curInjection.Value);
                    }
                    newChild.SetAttributeValue("lang", langCode);
                    parent.Add(newChild);
                }
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Save this wiki data as wiki files to a directory. All the existed files will be deleted.
        /// </summary>
        /// <param name="path">The output directory.</param>
        public void Save(string path) {
            if (this.allSetDict == null || this.allSetDict.Count == 0)
                return;

            if (Directory.Exists(path)) {
                DirectorySecurity ds = new DirectorySecurity(path, AccessControlSections.Access);
                if (ds.AreAccessRulesProtected) {
                    Log.Error();
                    Log.WriteLine("Outputing Wiki Data failure: No write permission to directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    return;
                } else {
                    DirectoryHelper.CleanDirectory(path, "*.wiki");
                }
            } else {
                try {
                    Directory.CreateDirectory(path);
                } catch (Exception ex) {
                    Log.Error();
                    Log.WriteLine("Outputing Wiki Data failure: Can not create directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    Log.Indent();
                    Log.WriteLine(ex.Message);
                    return;
                }
            }

            Log.Info();
            Log.Write("Start outputing Wiki Data: ");
            Log.WriteLine(ConsoleColor.Cyan, path);

            int countValidFiles = 0;
            int countInvalidFiles = 0;
            foreach (KeyValuePair<string, SortedDictionary<string, SetDict>> defTypeSubDict in this.allSetDict) {
                string defTypeName = defTypeSubDict.Key;
                SortedDictionary<string, SetDict> subDict = defTypeSubDict.Value;
                //string subDirPath = Path.Combine(path, defTypeName);
                //if (Directory.Exists(subDirPath))
                //{
                //    DirectorySecurity curDs = new DirectorySecurity(subDirPath, AccessControlSections.Access);
                //    if (curDs.AreAccessRulesProtected)
                //    {
                //        Log.Error();
                //        Log.WriteLine("Outputing to sub-directory failed: No write permission to directory.");
                //        Log.Indent();
                //        Log.WriteLine(ConsoleColor.Red, subDirPath);
                //        countInvalidFiles += subDict.Count;
                //        continue;
                //    }
                //}
                //else
                //{
                //    Directory.CreateDirectory(subDirPath);
                //}
                foreach (KeyValuePair<string, SetDict> pageNameSetDict in subDict) {
                    string filePath = Path.Combine(path, "Core_2_" + pageNameSetDict.Key + ".wiki");
                    SetDict setDict = pageNameSetDict.Value;
                    try {
                        setDict.Save(filePath);
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
            }
            if (countValidFiles > 0) {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine($"Completed outputing Wiki Data: {countValidFiles} files.");
                } else {
                    Log.Warning();
                    Log.WriteLine($"Completed outputing Wiki Data: Success: {countValidFiles} files, Failure {countInvalidFiles} files.");
                }
            } else {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine("No Wiki Data to be output.");
                } else {
                    Log.Error();
                    Log.WriteLine($"Outputing Wiki Data failure: {countInvalidFiles} files.");
                }
            }
        }

        /// <summary>
        /// Save wiki data as a CSV file.
        /// </summary>
        /// <param name="path"></param>
        public void SaveCSV(string path) {
            if (this.allSetDict == null || this.allSetDict.Count == 0)
                return;

            string dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir)) {
                DirectorySecurity ds = new DirectorySecurity(dir, AccessControlSections.Access);
                if (ds.AreAccessRulesProtected) {
                    Log.Error();
                    Log.WriteLine("Outputing Wiki Data failure: No write permission to directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, dir);
                    return;
                }
            } else {
                try {
                    Directory.CreateDirectory(dir);
                } catch (Exception ex) {
                    Log.Error();
                    Log.WriteLine("Outputing Wiki Data failure: Can not create directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, dir);
                    Log.Indent();
                    Log.WriteLine(ex.Message);
                    return;
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (var kvpPageNameSetDict in from sd in this.allSetDict.Values
                                               from kvp in sd
                                               select kvp) {
                sb.Append("Core:");
                sb.Append(kvpPageNameSetDict.Key);
                sb.AppendLine(",\"{{#set:");
                sb.AppendLine("|dataType=Def");
                sb.Append("|defType=");
                sb.AppendLine(kvpPageNameSetDict.Value.DefType);
                sb.Append("|defName=");
                sb.AppendLine(kvpPageNameSetDict.Value.DefName);
                foreach (var kvp in kvpPageNameSetDict.Value.Dict) {
                    sb.Append('|');
                    sb.Append(kvp.Key);
                    sb.Append('=');
                    sb.AppendLine(kvp.Value.Replace("\"", "\"\""));
                }
                sb.AppendLine("}}\"");
            }


            try {
                File.WriteAllText(path, sb.ToString(), utf8);
                Log.Info();
                Log.Write($"Completed outputing Wiki Data to CSV file: ");
                Log.WriteLine(ConsoleColor.Cyan, path);
            } catch (Exception ex) {
                Log.Error();
                Log.Write($"Outputing Wiki Data to SCV file failure, file: ");
                Log.WriteLine(ConsoleColor.Red, path);
                Log.Indent();
                Log.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}
