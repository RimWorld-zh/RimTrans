using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RimTrans.ModX
{
    public static class LoaderX
    {
        /// <summary>
        /// Load all xml files in Defs folder, return dictionary Defs. 
        /// </summary>
        public static Dictionary<string, XDocument> LoadDefs(this Dictionary<string, XDocument> defs, string pathDefs)
        {
            defs.Clear();
            if (Directory.Exists(pathDefs))
            {
                DirectoryInfo dirDefs = new DirectoryInfo(pathDefs);
                foreach (FileInfo file in from f in dirDefs.GetFiles("*", SearchOption.AllDirectories)
                                          where f.Extension == ".xml"
                                          select f)
                {
                    try
                    {
                        defs.Add(file.Directory.Name + "\\" + file.Name, XDocument.Load(file.FullName, LoadOptions.SetBaseUri));
                    }
                    catch (XmlException ex)
                    {
                        //TODO: Log
                    }
                }
            }
            return defs;
        }

        /// <summary>
        /// Laod DefInjected
        /// </summary>
        public static Dictionary<string, XDocument> LoadDefInjected(this Dictionary<string, XDocument> defInjected, string pathDefInjected)
        {
            defInjected.Clear();
            if (Directory.Exists(pathDefInjected))
            {
                DirectoryInfo dirDefInjected = new DirectoryInfo(pathDefInjected);
                foreach (DirectoryInfo subDir in dirDefInjected.GetDirectories())
                {
                    foreach (FileInfo file in from f in subDir.GetFiles()
                                              where f.Extension == ".xml"
                                              select f)
                    {
                        try
                        {
                            defInjected.Add(subDir.Name + "\\" + file.Name, XDocument.Load(file.FullName, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri));
                        }
                        catch (XmlException ex)
                        {
                            //TODO: Log
                        }
                    }
                }
            }
            return defInjected;
        }

        /// <summary>
        /// Load Keyed
        /// </summary>
        public static Dictionary<string, XDocument> LoadKeyed(this Dictionary<string, XDocument> keyed, string pathKeyed)
        {
            keyed.Clear();
            if (Directory.Exists(pathKeyed))
            {
                DirectoryInfo dirKeyed = new DirectoryInfo(pathKeyed);
                foreach (FileInfo file in from f in dirKeyed.GetFiles()
                                          where f.Extension == ".xml"
                                          select f)
                {
                    try
                    {
                        keyed.Add(file.Name, XDocument.Load(file.FullName, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri));
                    }
                    catch (XmlException ex)
                    {
                        //TODO: Log
                    }
                }
            }
            return keyed;
        }
    }
}
