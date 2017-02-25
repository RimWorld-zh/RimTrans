using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RimTrans.Builder.Xml
{
    public static class XmlHelper
    {
        public static bool HasField_defName(this XElement def)
        {
            foreach (XElement field in def.Elements())
            {
                if (string.Compare(field.Name.ToString(), "defName", true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static XElement defName(this XElement def)
        {
            foreach (XElement field in def.Elements())
            {
                if (string.Compare(field.Name.ToString(), "defName", true) == 0)
                {
                    return field;
                }
            }
            return null;
        }

        public static XElement Field(this XElement def, string fieldName)
        {
            foreach (XElement field in def.Elements())
            {
                if (string.Compare(field.Name.ToString(), fieldName, true) == 0)
                {
                    return field;
                }
            }
            return null;
        }
    }
}
