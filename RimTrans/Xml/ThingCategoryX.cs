using System;
using System.Linq;
using System.Xml.Linq;

namespace RimTrans.Xml
{
    public static class ThingCategoryX
    {
        public static ThingCategory Category(this XElement def)
        {
            
            var category = def.Field(FieldType.category);
            if (category != null)
            {
                ThingCategory thingCategory;
                Enum.TryParse<ThingCategory>(category.Value, true, out thingCategory); // Ignore Case
                return thingCategory;
            }
            else
            {
                return ThingCategory.None;
            }
        }
    }
}
