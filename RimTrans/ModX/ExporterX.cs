using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace RimTrans.ModX
{
    public static class ExporterX
    {
        /// <summary>
        /// Export all files of this dictionary.
        /// </summary>
        /// <param name="path">path of the target folder</param>
        public static void Export(this Dictionary<string, XDocument> dict, string path)
        {
            try
            {
                foreach (var kvp in dict)
                {
                    string pathFile = Path.Combine(path, kvp.Key);
                    string defType = kvp.Key.Substring(0, kvp.Key.IndexOf('\\'));
                    string pathFolder = Path.GetDirectoryName(pathFile);
                    if (Directory.Exists(pathFolder) == false)
                    {
                        Directory.CreateDirectory(pathFolder);
                    }
                    kvp.Value.Save(pathFile);
                }
            }
            catch (Exception)
            {
                //TODO: Log
            }
        }
        
        /// <summary>
        /// Delete invalid files of this dictionary in this path;
        /// </summary>
        public static void DeleteInvalidFiles(this Dictionary<string, XDocument> dict, string path)
        {
            try
            {
                foreach (var kvp in dict)
                {
                    string pathFile = Path.Combine(path, kvp.Key);
                    string defType = kvp.Key.Substring(0, kvp.Key.IndexOf('\\'));
                    string pathFolder = Path.GetDirectoryName(pathFile);
                    if (kvp.Value.Root.Elements().Count() == 0)
                    {
                        if (Config.IsFilesInvalidDelete)
                        {
                            if (File.Exists(pathFile)) File.Delete(pathFile);
                        }
                        else
                        {
                            kvp.Value.Save(pathFile);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //TODO: Log
            }
        }

        /// <summary>
        /// Recursively delete all empty folder in this directory.
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteEmptyFolders(this string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var subDir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        Directory.Delete(subDir);
                    }
                    catch (Exception)
                    {
                    }
                }
                try
                {
                    Directory.Delete(path);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Convert the entity reference "&amp;gt;" to the greater-than sign "&gt;"
        /// </summary>
        public static void ConvertEntityReference(this string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo Dir = new DirectoryInfo(path);
                foreach (var subDir in Dir.GetDirectories())
                {
                    if (subDir.Name == "InteractionDef" ||
                        subDir.Name == "RulePackDef" ||
                        subDir.Name == "TaleDef")
                    {
                        foreach (var file in from f in subDir.GetFiles()
                                             where f.Extension == ".xml"
                                             select f)
                        {
                            string text = string.Empty;
                            using (StreamReader sr = new StreamReader(file.FullName))
                            {
                                text = sr.ReadToEnd();
                            }
                            text = text.Replace("&gt;", ">");
                            using (StreamWriter sw = new StreamWriter(file.FullName))
                            {
                                sw.Write(text);
                            }
                        }
                    }
                }
            }
        }


    }
}
