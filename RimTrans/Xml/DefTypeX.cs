using System;
using System.Linq;
using System.Xml.Linq;

namespace RimTrans.Xml
{
    public static class DefTypeX
    {
        public static DefType GetDefType(this XElement def)
        {
            DefType defType;
            Enum.TryParse<DefType>(def.Name.ToString(), out defType);
            return defType;
        }
    }
}
