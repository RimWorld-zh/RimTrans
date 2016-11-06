using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RimTrans.Xml;

namespace RimTrans.ModX
{
    internal static class PreProcessorX
    {
        #region abstract and inheritance

        /// <summary>
        /// Extract abstracts from this dictionary Defs.
        /// </summary>
        public static IEnumerable<XElement> ExtractAbstrs(this Dictionary<string, XDocument> defs)
        {
            var abstrs = from doc in defs.Values
                         from ele in doc.Root.Elements()
                         where ele.AbstrName() != null
                         select ele;
            return abstrs;
        }

        /// <summary>
        /// Query the abstract in this abstracts sheet.
        /// </summary>
        /// <param name="elementDef">Find the parent of elementDef.</param>
        private static XElement QueryAbstr(this XElement abstrsSheet, XElement def)
        {
            // Because of overwrite, do not break, find the last one.
            XElement abstr = null;
            foreach (var ele in abstrsSheet.Defs(def.GetDefType()))
            {
                if (ele.AbstrName() == def.AbstrParentName()) abstr = ele;
            }
            return abstr;
        }

        /// <summary>
        /// Inherit abstracts to this dictionary Defs.
        /// </summary>
        /// <param name="abstrs">Document storges abstracts.</param>
        public static Dictionary<string, XDocument> Inherit(this Dictionary<string, XDocument> defs, XElement abstrsSheet)
        {
            foreach (var doc in defs.Values)
            {
                foreach (var def in from d in doc.Root.Elements()
                                    where d.AbstrParentName() != null && d.HasDefName()
                                    select d)
                {
                    XElement abstr = abstrsSheet.QueryAbstr(def); ;
                    while (abstr != null)
                    {
                        def.Inherit(abstr);
                        abstr = abstrsSheet.QueryAbstr(abstr);
                    }
                }
            }
            return defs;
        }

        /// <summary>
        /// Recursively inherit.
        /// </summary>
        private static XElement Inherit(this XElement def, XElement abstr)
        {
            foreach (var childAbstr in abstr.Elements())
            {
                bool isExisting = false;
                foreach (var childDef in def.Elements())
                {
                    if (childAbstr.NameLower() == childDef.NameLower())
                    {
                        isExisting = true;
                        if (childAbstr.IsArray() && childDef.IsArray())
                        {
                            childDef.AddFirst(childAbstr.Elements()); // Inherit collection of <li>, must add first
                        }
                        else if (childAbstr.HasElements && childDef.HasElements)
                        {
                            childDef.Inherit(childAbstr); // Recursively inherit
                        }
                        break;
                    }
                }
                if (isExisting == false)
                {
                    def.Add(childAbstr);
                }
            }
            return def;
        }
        
        #endregion

        #region Patch

        /// <summary>
        /// Extract genders infomation of all pawn.
        /// </summary>
        public static IEnumerable<XElement> ExtractGenders(this Dictionary<string, XDocument> defs)
        {
            List<XElement> gendersInfos = new List<XElement>();
            foreach (var doc in defs.Values)
            {
                foreach (var def in from d in doc.Root.Elements()
                                    where d.GetDefType() == DefType.ThingDef && d.Category() == ThingCategory.Pawn && d.HasDefName()
                                    select d)
                {
                    XElement defName = def.Field(FieldType.defName);
                    XElement race = def.Field(FieldType.race);
                    if (race != null)
                    {
                        XElement hasGenders = race.Field(FieldType.hasGenders);
                        if (hasGenders != null)
                        {
                            gendersInfos.Add(new XElement(defName.Value, hasGenders.Value));
                        }
                        else
                        {
                            gendersInfos.Add(new XElement(defName.Value, false));
                        }
                    }
                }
            }
            return gendersInfos;
        }

        /// <summary>
        /// Query genders information from this genders cheat sheet.
        /// </summary>
        /// <param name="race">the race that you want to query</param>
        public static bool QueryGenders(this XElement GendersSheet, XElement race)
        {
            bool result;
            XElement info = null;
            foreach (var ele in GendersSheet.Elements())
            {
                if (ele.Name.ToString().ToLower() == race.Value.ToLower()) info = ele; // Find the last one;
            }
            if (info != null && bool.TryParse(info.Value, out result)) return result;
            return false;
        }

        /// <summary>
        /// Patch fields &lt;PawnKindDef.lifestages.li.label/labelMale/labelFemale&gt;
        /// </summary>
        public static Dictionary<string, XDocument> PatchPawnGenders(this Dictionary<string, XDocument> defs, XElement gendersSheet)
        {
            foreach (var doc in defs.Values)
            {
                foreach (var def in from d in doc.Root.Defs(DefType.PawnKindDef)
                                    where d.HasDefName()
                                    select d)
                {
                    XElement race = def.Field(FieldType.race);
                    XElement lifeStages = def.Field(FieldType.lifeStages);
                    if (race != null && lifeStages != null)
                    {
                        // root
                        XElement label = def.Field(FieldType.label);
                        if (label == null)
                        {
                            def.Add(new XElement(FieldType.label.ToString(), def.Field(FieldType.defName).Value));
                            label = def.Field(FieldType.label);
                        }
                        XElement labelMale = def.Field(FieldType.labelMale);
                        XElement labelFemale = def.Field(FieldType.labelFemale);
                        if (labelMale != null && labelFemale == null)
                        {
                            labelMale.AddAfterSelf(new XElement(FieldType.labelFemale.ToString()), label.Value);
                            labelFemale = def.Field(FieldType.labelFemale);
                        }
                        if (labelMale == null && labelFemale != null)
                        {
                            labelFemale.AddBeforeSelf(new XElement(FieldType.labelMale.ToString(), label.Value));
                            labelMale = def.Field(FieldType.labelMale);
                        }

                        int count = lifeStages.Elements().Count();
                        if (count >= 3)
                        {
                            // Patch root
                            if (labelMale == null)
                            {
                                def.Add(new XElement(FieldType.labelMale.ToString(), label.Value));
                                labelMale = def.Field(FieldType.labelMale);
                            }
                            if (labelFemale == null)
                            {
                                def.Add(new XElement(FieldType.labelFemale.ToString(), label.Value));
                                labelFemale = def.Field(FieldType.labelFemale);
                            }
                            
                            // Patch All lifeStages
                            int index = 0;
                            foreach (var li in lifeStages.Elements())
                            {
                                XElement liLabel = li.Field(FieldType.label);
                                if (liLabel == null)
                                {
                                    li.Add(new XElement(label));
                                    liLabel = li.Field(FieldType.label);
                                }

                                XElement liLabelMale = li.Field(FieldType.labelMale);
                                if (liLabelMale == null)
                                {
                                    li.Add(new XElement(FieldType.labelMale.ToString(), label.Value));
                                    liLabelMale = li.Field(FieldType.labelMale);
                                    if (index >=2 && labelMale != null)
                                    {
                                        liLabelMale.Value = labelMale.Value;
                                    }
                                }
                                else if (index >= 2)
                                {
                                    labelMale.Value = liLabelMale.Value;
                                }

                                XElement liLabelFemale = li.Field(FieldType.labelFemale);
                                if (liLabelFemale == null)
                                {
                                    li.Add(new XElement(FieldType.labelFemale.ToString(), label.Value));
                                    liLabelFemale = li.Field(FieldType.labelFemale);
                                    if (index >= 2 && labelFemale != null)
                                    {
                                        liLabelFemale.Value = labelFemale.Value;
                                    }
                                }
                                else if (index >= 2)
                                {
                                    labelFemale.Value = liLabelFemale.Value;
                                }

                                index++;
                            }

                            // Trim and Order
                            bool flagGenders = gendersSheet.QueryGenders(race) && Config.IsPatchPawnGenders;
                            foreach (var li in lifeStages.Elements())
                            {
                                XElement liLabel = li.Field(FieldType.label);
                                liLabel.Remove();
                                XElement liLabelMale = li.Field(FieldType.labelMale);
                                liLabelMale.Remove();
                                XElement liLabelFemale = li.Field(FieldType.labelFemale);
                                liLabelFemale.Remove();

                                if (flagGenders)
                                {
                                    li.AddFirst(liLabelMale);
                                    liLabelMale.AddAfterSelf(liLabelFemale);
                                }
                                else
                                {
                                    li.AddFirst(liLabel);
                                }
                            }

                            // Remove root labelMale and labelFemale
                            labelMale.Remove();
                            labelFemale.Remove();
                        }
                    }
                }
            }
            return defs;
        }

        /// <summary>
        /// Patch fields &lt;PawnRelationDef.labelFemale&gl;
        /// </summary>
        /// <param name="defs"></param>
        /// <returns></returns>
        public static Dictionary<string, XDocument> PatchPawnRelation(this Dictionary<string, XDocument> defs)
        {
            foreach (var doc in defs.Values)
            {
                foreach (var def in from d in doc.Root.Defs(DefType.PawnRelationDef)
                                    where d.HasDefName()
                                    select d)
                {
                    XElement label = def.Field(FieldType.label);
                    if (label != null &&
                        def.Field(FieldType.labelFemale) == null)
                    {
                        def.Add(new XElement(FieldType.labelFemale.ToString(), label.Value));
                    }
                }
            }
            return defs;
        }

        /// <summary>
        /// Patch fields &lt;ScenariaDef.scenario.name/description&gt;
        /// </summary>
        public static Dictionary<string, XDocument> PatchScenoria(this Dictionary<string, XDocument> defs)
        {
            foreach (var doc in defs.Values)
            {
                foreach (var def in from d in doc.Root.Defs(DefType.ScenarioDef)
                                    where d.HasDefName()
                                    select d)
                {
                    XElement label = def.Field(FieldType.label);
                    XElement description = def.Field(FieldType.description);
                    XElement scenario = def.Field(FieldType.scenario);
                    if (scenario != null)
                    {
                        if (label != null) scenario.Add(new XElement(FieldType.name.ToString(), label.Value));
                        if (description != null) scenario.Add(new XElement(description));
                    }
                }
            }
            return defs;
        }

        /// <summary>
        /// Patch fields &lt;SkillDef.label&gt;
        /// </summary>
        public static Dictionary<string, XDocument> PatchSkill(this Dictionary<string, XDocument> defs)
        {
            foreach (var doc in defs.Values)
            {
                foreach (var def in from d in doc.Root.Defs(DefType.SkillDef)
                                    where d.HasDefName()
                                    select d)
                {
                    XElement defName = def.Field(FieldType.defName);
                    XElement label = def.Field(FieldType.label);
                    if (label == null)
                    {
                        def.FirstNode.AddAfterSelf(new XElement(FieldType.label.ToString(), defName.Value));
                    }
                }
            }
            return defs;
        }

        /// <summary>
        /// Patch fields &lt;ThingDef.stuffProps.stuffAdjctive&gt;
        /// </summary>
        public static Dictionary<string, XDocument> PatchStuffAdjective(this Dictionary<string, XDocument> defs)
        {
            if (Config.IsPatchStuffAdjective == false) return defs;
            foreach (var doc in defs.Values)
            {
                foreach (var def in from d in doc.Root.Defs(DefType.ThingDef)
                                    where d.Category() ==  ThingCategory.Item && d.HasDefName()
                                    select d)
                {
                    XElement label = def.Field(FieldType.label);
                    XElement stuffProps = def.Field(FieldType.stuffProps);
                    if (label != null &&
                        stuffProps != null &&
                        stuffProps.Field(FieldType.categories) != null &&
                        stuffProps.Field(FieldType.stuffAdjective) == null)
                    {
                        stuffProps.AddFirst(new XElement(FieldType.stuffAdjective.ToString(), label.Value));
                    }
                }
            }
            return defs;
        }

        /// <summary>
        /// Add index number to fields &lt;li&gt;, attribute form like &lt;li Index="0"&gt;
        /// </summary>
        public static Dictionary<string, XDocument> SignIndex(this Dictionary<string, XDocument> defs)
        {
            foreach (var doc in defs.Values)
            {
                foreach (var def in from d in doc.Root.Elements()
                                    where d.HasDefName()
                                    select d)
                {
                    def.SignIndex();
                }
            }
            return defs;
        }

        /// <summary>
        /// Recursively sign index
        /// </summary>
        public static XElement SignIndex(this XElement element)
        {
            foreach (var field in element.Elements())
            {
                if (field.IsArray())
                {
                    int index = 0;
                    foreach (var li in field.Elements())
                    {
                        li.SetAttributeValue("Index", index);
                        li.SignIndex();
                        index++;
                    }
                }
                else
                {
                    field.SignIndex();
                }
            }
            return element;
        }

        #endregion
    }
}
