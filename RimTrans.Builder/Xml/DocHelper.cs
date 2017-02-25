using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RimTrans.Builder.Xml
{
    public static class DocHelper
    {
        public static XDocument EmptyDoc()
        {
            return XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<LanguageData>\r\n\r\n</LanguageData>", LoadOptions.PreserveWhitespace);
        }

        public static XDocument EmptyDocEx()
        {
            return XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<LanguageData LastIsSingle=\"false\">\r\n\r\n</LanguageData>", LoadOptions.PreserveWhitespace);
        }

        public static XDocument LoadLanguageDoc(string path)
        {
            XDocument doc = XDocument.Load(path, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri);
            foreach (XText text in from node in doc.Root.Nodes()
                                   where node.NodeType == XmlNodeType.Text
                                   select node)
            {
                text.Value = text.Value.Replace(" ", "").Replace("\t", "") + "  ";
            }
            XNode lastNode = doc.Root.LastNode;
            if (lastNode.NodeType == XmlNodeType.Text)
            {
                (lastNode as XText).Value = (lastNode as XText).Value.Replace("  ", "");
            }
            else
            {
                doc.Root.Add("\n");
            }
            return doc;
        }

        public static void CommentAll(this XDocument doc)
        {
            while (doc.Root.HasElements)
            {
                XElement firstElement = doc.Root.Elements().First();
                firstElement.ReplaceWith(new XComment(firstElement.ToString()));
            }
        }
    }
}
