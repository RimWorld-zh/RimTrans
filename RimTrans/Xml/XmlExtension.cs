using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace RimTrans.Xml
{
    public static class XmlExtension
    {
        /// <summary>
        /// Get the lowercase name of this element, because RimWorld fields are not case sensitive.
        /// </summary>
        public static string NameLower(this XElement element)
        {
            return element.Name.ToString().ToLower();
        }

        /// <summary>
        /// Get the collection of Defs, speciflic DefType.
        /// </summary>
        public static IEnumerable<XElement> Defs(this XElement root, DefType defType)
        {
            var defs = from d in root.Elements()
                       where d.GetDefType() == defType
                       select d;
            return defs;
        }

        /// <summary>
        /// Get the first field, speciflic FieldType.
        /// </summary>
        public static XElement Field(this XElement element, FieldType fieldType)
        {
            foreach (var child in element.Elements())
            {
                if (child.GetFieldType() == fieldType) return child;
            }
            return null;
        }

        /// <summary>
        /// Get the collection of fields, speciflic FieldType.
        /// </summary>
        public static IEnumerable<XElement> Fields(this XElement element, FieldType fieldType)
        {
            var fields = from f in element.Elements()
                         where f.GetFieldType() == fieldType
                         select f;
            return fields;
        }

        /// <summary>
        /// If the Def has &lt;defName&gt; fields.
        /// </summary>
        public static bool HasDefName(this XElement def)
        {
            return def.Field(FieldType.defName) != null;
        }

        /// <summary>
        /// If the fields is all &lt;li&gt; fields.
        /// </summary>
        public static bool IsArray(this XElement element)
        {
            return element.HasElements && element.Elements().Count() == element.Fields(FieldType.li).Count();
        }

        /// <summary>
        /// Get the value of the attribute Name.
        /// </summary>
        public static string AbstrName(this XElement def)
        {
            foreach (var attribute in def.Attributes())
            {
                if (attribute.Name.ToString().ToLower() == "name") return attribute.Value;
            }
            return null;
        }

        /// <summary>
        /// Get the value of the attribute ParentName.
        /// </summary>
        public static string AbstrParentName(this XElement def)
        {
            foreach (var attribute in def.Attributes())
            {
                if (attribute.Name.ToString().ToLower() == "parentname") return attribute.Value;
            }
            return null;
        }

        /// <summary>
        /// If this comment is valid, has content.
        /// </summary>
        public static bool IsValid(this XComment comment)
        {
            bool result = false;
            foreach (Match match in Regex.Matches(comment.Value, "[A-Za-z0-9]{1,}"))
            {
                result = true;
                break;
            }
            return result;
        }

        /// <summary>
        /// Remove redundant equel-sign of this comment's value
        /// </summary>
        public static string TrimComm(this string commentValue)
        {
            bool flag = true;
            string result = commentValue;
            while (flag)
            {
                flag = false;
                foreach (Match match in Regex.Matches(result, "[=]{3,}"))
                {
                    result = result.Remove(match.Index, match.Length);
                    flag = true;
                    break;
                }
            }
            result = result.Trim();
            result = " " + result + " ";
            return result;
        }

        /// <summary>
        /// Check if this defName is valid.
        /// </summary>
        public static bool IsValidDefName(this XElement defName)
        {
            bool result = false;
            try
            {
                XElement test = new XElement(defName.Value);
                result = true;
            }
            catch (XmlException)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Get this element's field name
        /// </summary>
        public static string FieldName(this XElement element)
        {
            FieldType fieldType = element.GetFieldType();
            string fieldName;
            if (fieldType == FieldType.li)
                fieldName = element.Attribute("Index").Value;
            else
                fieldName = fieldType.ToString();
            return fieldName;
        }
    }
}
