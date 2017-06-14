using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RimTrans.Builder.Xml {
    public static class DocHelper {
        public static XDocument EmptyDoc() {
            return XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<LanguageData>\r\n\r\n</LanguageData>", LoadOptions.PreserveWhitespace);
        }

        public static XDocument EmptyDocEx() {
            return XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<LanguageData LastIsSingle=\"false\">\r\n\r\n</LanguageData>", LoadOptions.PreserveWhitespace);
        }

        public static XDocument EmptyDocDef() {
            return XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?><Defs></Defs>");
        }

        public static XDocument LoadLanguageDoc(string path) {
            XDocument doc = XDocument.Load(path, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri);
            XElement root = doc.Root;
            foreach (XText text in from node in root.Nodes()
                                   where node.NodeType == XmlNodeType.Text
                                   select node) {
                text.Value = text.Value.Replace(" ", "").Replace("\t", "") + "  ";
            }
            XNode lastNode = root.LastNode;
            if (lastNode.NodeType == XmlNodeType.Text) {
                XText lastText = lastNode as XText;
                lastText.Value = lastText.Value.Replace("  ", "");
                if (lastText.Value.Length == 1) {
                    lastText.Value += "\n";
                }

            } else {
                root.Add("\n\n");
            }
            return doc;
        }

        public static XDocument ToCommentDoc(this XDocument doc) {
            XDocument commentDoc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<LanguageData></LanguageData>", LoadOptions.PreserveWhitespace);
            XElement commentRoot = commentDoc.Root;
            XElement root = doc.Root;
            foreach (XNode node in root.Nodes()) {
                if (node.NodeType == XmlNodeType.Element) {
                    commentRoot.Add(new XComment(node.ToString()));
                } else {
                    commentRoot.Add(node);
                }
            }
            return commentDoc;
        }
    }
}
