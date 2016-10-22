using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using RimTrans.Xml;

namespace RimTrans.ModX
{
    internal static class MatcherX
    {
        /// <summary>
        /// Get the snapshot of the existing DefInjected.
        /// </summary>
        public static XElement GetInjectionsSheet(this Dictionary<string, XDocument> defInjectedExisting)
        {
            XElement injectionsSheet = new XElement("Injections");
            foreach (var kvp in defInjectedExisting)
            {
                string defType = kvp.Key.Substring(0, kvp.Key.IndexOf('\\'));
                if (injectionsSheet.Element(defType) == null) injectionsSheet.Add(new XElement(defType));
                injectionsSheet.Element(defType).Add(kvp.Value.Root.Elements());
            }
            return injectionsSheet;
        }

        /// <summary>
        /// Match the DefInjected of Core. Comment the redundant fields.
        /// </summary>
        public static void MatchCore(this Dictionary<string, XDocument> defInjected, XElement injectionsSheet)
        {
            if (injectionsSheet == null) return;

            foreach (var kvp in defInjected)
            {
                string defType = kvp.Key.Substring(0, kvp.Key.IndexOf('\\'));
                XElement group = injectionsSheet.Element(defType);
                if (group != null)
                {
                    List<XElement> matches = new List<XElement>();
                    foreach (var fieldMod in kvp.Value.Root.Elements())
                    {
                        foreach (var fieldCore in group.Elements())
                        {
                            if (fieldMod.Name == fieldCore.Name)
                            {
                                fieldMod.Value = fieldCore.Value;
                                matches.Add(fieldMod);
                            }
                        }
                    }
                    foreach (var match in matches)
                    {
                        match.ReplaceWith(new XComment(" [Core] " + match.ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// Match the existing DefInjected. Adopt.
        /// </summary>
        /// <param name="defInjected"></param>
        /// <param name="injectionsSheet"></param>
        public static void MatchExisting(this Dictionary<string, XDocument> defInjected, XElement injectionsSheet)
        {
            if (injectionsSheet == null) return;

            foreach (var kvp in defInjected)
            {
                string defType = kvp.Key.Substring(0, kvp.Key.IndexOf('\\'));
                XElement group = injectionsSheet.Element(defType);
                if (group != null)
                {
                    foreach (var fieldNew in kvp.Value.Root.Elements())
                    {
                        foreach (var fieldExisting in group.Elements())
                        {
                            if (fieldNew.Name == fieldExisting.Name)
                            {
                                fieldNew.Value = fieldExisting.Value;
                                //Console.WriteLine(fieldNew);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Hold the invalid fields or delete the invalid files.
        /// </summary>
        public static void MatchFiles(this Dictionary<string, XDocument> defInjectedNew, Dictionary<string, XDocument> defInjectedExisting)
        {
            foreach (var kvp in defInjectedExisting)
            {
                XDocument docExisting = kvp.Value;
                XDocument docNew;
                if (defInjectedNew.TryGetValue(kvp.Key, out docNew))
                {
                    var nodesExisting = from n in docExisting.Root.Nodes()
                                        where n.NodeType == XmlNodeType.Comment || n.NodeType == XmlNodeType.Element
                                        select n;
                    IEnumerable<XComment> commsNew = from n in docNew.Root.Nodes()
                                                     where n.NodeType == XmlNodeType.Comment
                                                     select n as XComment;
                    var fieldsNew = docNew.Root.Elements();

                    bool hasInvalid = false;
                    foreach (var nodeExisting in nodesExisting)
                    {
                        if (nodeExisting.NodeType == XmlNodeType.Comment)
                        {
                            XComment commExisting = nodeExisting as XComment;
                            bool invalid = true;
                            foreach (var commNew in commsNew)
                            {
                                if (commExisting.Value == commNew.Value)
                                {
                                    invalid = false;
                                    break;
                                }
                            }
                            if (invalid)
                            {
                                docNew.Root.Add(Config.IndentSpaces, commExisting, "\n");
                                hasInvalid = true;
                            }
                        }
                        else if (nodeExisting.NodeType == XmlNodeType.Element)
                        {
                            XElement fieldExisting = nodeExisting as XElement;
                            bool invalid = true;
                            foreach (var fieldNew in fieldsNew)
                            {
                                if (fieldExisting.NameLower() == fieldNew.NameLower())
                                {
                                    invalid = false;
                                    break;
                                }
                            }
                            if (invalid)
                            {
                                docNew.Root.Add(Config.IndentSpaces, new XComment(fieldExisting.ToString()), "\n");
                                hasInvalid = true;
                            }
                        }
                    }

                    if (hasInvalid) docNew.Root.Add("\n");
                }
                else
                {
                    docNew = new XDocument(docExisting);
                    List<XElement> invalids = new List<XElement>();
                    invalids.AddRange(docNew.Root.Elements());
                    foreach (var fieldInvalid in invalids)
                    {
                        fieldInvalid.ReplaceWith(new XComment(fieldInvalid.ToString()));
                    }
                }
            }
        }

        /// <summary>
        /// Match self. Comment the redundant fields.
        /// </summary>
        public static void MatchSelf(this Dictionary<string, XDocument> defInjected)
        {
            XElement selfSheet = new XElement("SelfSheet");
            foreach (var kvp in defInjected)
            {
                string defType = kvp.Key.Substring(0, kvp.Key.IndexOf('\\'));
                if (selfSheet.Element(defType) == null) selfSheet.Add(new XElement(defType));
                selfSheet.Element(defType).Add(kvp.Value.Root.Elements());
            }
            foreach (var kvp in defInjected)
            {
                string defType = kvp.Key.Substring(0, kvp.Key.IndexOf('\\'));
                XElement group = selfSheet.Element(defType);
                List<XElement> matches = new List<XElement>();
                foreach (var field in kvp.Value.Root.Elements())
                {
                    if (group.Elements(field.Name).Count() > 1)
                    {
                        group.Elements(field.Name).First().Remove();
                        matches.Add(field);
                    }
                }
                foreach (var field in matches)
                {
                    field.ReplaceWith(new XComment(" [Redundant] " + field.ToString()));
                }
            }
        }
    }
}
