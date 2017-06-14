using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using RimTrans.Builder;
using RimTrans.Builder.Xml;

namespace RimTrans.Builder.Wiki {
    public class WikiKeyed {
        private static readonly UTF8Encoding utf8 = new UTF8Encoding(false);

        private SortedDictionary<string, StringBuilder> jsons;
        private SortedDictionary<string, StringBuilder> tables;
        private SortedDictionary<string, Dictionary<string, string>> dicts;

        WikiKeyed() {
            this.jsons = new SortedDictionary<string, StringBuilder>();
            this.tables = new SortedDictionary<string, StringBuilder>();
            this.dicts = new SortedDictionary<string, Dictionary<string, string>>();
        }

        public static WikiKeyed Parse(params KeyedData[] keyedDatas) {
            Log.Info();
            Log.Write("Start parsing KeyedDatas generating ");
            Log.Write(ConsoleColor.Cyan, "Wiki Keyed");
            Log.WriteLine(".");

            WikiKeyed wikiKeyed = new WikiKeyed();

            int countKey = 0;
            foreach (KeyedData curKeyedData in keyedDatas) {
                StringBuilder json = new StringBuilder();
                json.Append("{\n");
                StringBuilder table = new StringBuilder();
                table.Append("return {\n");
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (XElement ele in from doc in curKeyedData.Data.Values
                                         from ele in doc.Root.Elements()
                                         select ele) {
                    string key = ele.Name.ToString();
                    string value = ele.Value.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\\n", "<br/>");
                    // JSON
                    json.Append("    \"");
                    json.Append(key);
                    json.Append("\": \"");
                    json.Append(value.Replace("\\", "\\\\").Replace("\"", "\\\""));
                    json.AppendLine("\",");
                    // Lua table
                    table.Append("    ");
                    if (key == "repeat") {
                        table.Append("-- " + key);
                    } else {
                        table.Append(key);
                    }
                    table.Append(" = \"");
                    table.Append(value.Replace("\\", "\\\\").Replace("\"", "\\\""));
                    table.AppendLine("\",");
                    // SMW
                    dict.Add("Keyed." + key, value.Replace("|", "{{!}}"));
                    countKey++;
                }
                json.Remove(json.Length - 3, 1);
                json.Append("}");
                table.Remove(table.Length - 3, 1);
                table.Append("}");
                string name = curKeyedData.Name;
                while (wikiKeyed.jsons.ContainsKey(name)) {
                    name += "_";
                }
                wikiKeyed.jsons.Add(name, json);
                wikiKeyed.tables.Add(name, table);
                wikiKeyed.dicts.Add(name, dict);
            }

            Log.Info();
            Log.WriteLine($"Complete generating wiki keyed: {wikiKeyed.jsons.Count} KeyData, {countKey} keys.");

            return wikiKeyed;
        }

        /// <summary>
        /// Save wiki keyed file to a directory.
        /// </summary>
        /// <param name="path">path to the directory</param>
        public void Save(string path) {
            if (this.jsons == null || this.jsons.Count == 0)
                return;

            if (Directory.Exists(path)) {
                DirectorySecurity ds = new DirectorySecurity(path, AccessControlSections.Access);
                if (ds.AreAccessRulesProtected) {
                    Log.Error();
                    Log.WriteLine("Outputing Wiki Keyed failure: No write permission to directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    return;
                } else {
                    DirectoryHelper.CleanDirectory(path, "*");
                }
            } else {
                try {
                    Directory.CreateDirectory(path);
                } catch (Exception ex) {
                    Log.Error();
                    Log.WriteLine("Outputing Wiki Keyed failure: Can not create directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    Log.Indent();
                    Log.WriteLine(ex.Message);
                    return;
                }
            }

            Log.Info();
            Log.Write("Start outputing Wiki Keyed: ");
            Log.WriteLine(ConsoleColor.Cyan, path);

            int countValidFiles = 0;
            int countInvalidFiles = 0;
            // JSON
            foreach (var fileNameJsonPair in this.jsons) {
                string fileName = Path.Combine(path, fileNameJsonPair.Key);
                try {
                    File.WriteAllText(fileName + ".json", fileNameJsonPair.Value.ToString(), utf8);
                    countValidFiles++;
                } catch (Exception) {
                    countInvalidFiles++;
                }
            }
            // Lua table
            foreach (var fileNameTablePair in this.tables) {
                string fileName = Path.Combine(path, fileNameTablePair.Key);
                try {
                    File.WriteAllText(fileName + ".lua", fileNameTablePair.Value.ToString(), utf8);
                    countValidFiles++;
                } catch (Exception) {
                    countInvalidFiles++;
                }
            }
            // SMW
            foreach (var fileNameDictPair in this.dicts) {
                string fileName = Path.Combine(path, fileNameDictPair.Key);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{{#set:");
                sb.AppendLine("|dataType=Keyed");
                sb.AppendLine($"|KeyedLanguage={fileNameDictPair.Key}");
                Dictionary<string, string> dict = fileNameDictPair.Value;
                foreach (var kvp in dict) {
                    sb.Append('|');
                    sb.Append(kvp.Key);
                    sb.Append('=');
                    sb.AppendLine(kvp.Value);
                }
                sb.Append("}}");
                try {
                    File.WriteAllText(fileName + ".wiki", sb.ToString(), utf8);
                    countValidFiles++;
                } catch (Exception) {
                    countInvalidFiles++;
                }
            }
            if (countValidFiles > 0) {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine($"Completed outputing Wiki Keyed: {countValidFiles} files.");
                } else {
                    Log.Warning();
                    Log.WriteLine($"Completed outputing Wiki Keyed: Success: {countValidFiles} files, Failure {countInvalidFiles} files.");
                }
            } else {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine("No Wiki Keyed to be output.");
                } else {
                    Log.Error();
                    Log.WriteLine($"Outputing Wiki Keyed failure: {countInvalidFiles} files.");
                }
            }
        }

        /// <summary>
        /// Save wiki keyed file as lua table to a directory.
        /// </summary>
        /// <param name="path">path to the directory</param>
        public void SaveLuaTable(string path) {
            if (this.tables == null || this.tables.Count == 0)
                return;

            if (Directory.Exists(path)) {
                DirectorySecurity ds = new DirectorySecurity(path, AccessControlSections.Access);
                if (ds.AreAccessRulesProtected) {
                    Log.Error();
                    Log.WriteLine("Outputing Wiki Keyed failure: No write permission to directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    return;
                } else {
                    DirectoryHelper.CleanDirectory(path, "*");
                }
            } else {
                try {
                    Directory.CreateDirectory(path);
                } catch (Exception ex) {
                    Log.Error();
                    Log.WriteLine("Outputing Wiki Keyed failure: Can not create directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    Log.Indent();
                    Log.WriteLine(ex.Message);
                    return;
                }
            }

            Log.Info();
            Log.Write("Start outputing Wiki Keyed: ");
            Log.WriteLine(ConsoleColor.Cyan, path);

            int countValidFiles = 0;
            int countInvalidFiles = 0;
            foreach (var LangNameTablePair in this.tables) {
                string filePath = Path.Combine(path, "Keyed_" + LangCodeHelper.GetCodeShort(LangNameTablePair.Key) + "_dict.lua");
                try {
                    File.WriteAllText(filePath, LangNameTablePair.Value.ToString(), utf8);
                    countValidFiles++;
                } catch (Exception) {
                    countInvalidFiles++;
                }
            }
            if (countValidFiles > 0) {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine($"Completed outputing Wiki Keyed: {countValidFiles} files.");
                } else {
                    Log.Warning();
                    Log.WriteLine($"Completed outputing Wiki Keyed: Success: {countValidFiles} files, Failure {countInvalidFiles} files.");
                }
            } else {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine("No Wiki Keyed to be output.");
                } else {
                    Log.Error();
                    Log.WriteLine($"Outputing Wiki Keyed failure: {countInvalidFiles} files.");
                }
            }
        }
    }
}
