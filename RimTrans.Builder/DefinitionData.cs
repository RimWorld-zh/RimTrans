using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RimTrans.Builder.Xml;

namespace RimTrans.Builder
{
    public class DefinitionData
    {
        public string _path = string.Empty;
        public string Path { get { return this._path; } }

        private SortedDictionary<string, XDocument> _data;
        public SortedDictionary<string, XDocument> Data { get { return this._data; } }

        private XElement _abstracts;


        private DefinitionData()
        {

        }

        #region Load

        public static DefinitionData Load(string path, DefinitionData definitionDataCore = null)
        {
            DefinitionData definitionData = new DefinitionData();
            definitionData._path = path;

            definitionData.Load(path);

            definitionData.Inherit(definitionDataCore);

            definitionData.HandleDetails();

            definitionData.CompletePawnKindLabel(definitionDataCore);
            definitionData.CompletePawnRelationLabel();
            definitionData.CompleteScenarioNameAndDesc();
            definitionData.CompleteSkillLabel();
            definitionData.CompleteThingDefStuffAdjective();
            definitionData.CompleteWorkTypeLabel();

            return definitionData;
        }

        private void Load(string path)
        {
            this._data = new SortedDictionary<string, XDocument>();
            this._abstracts = new XElement("Abstracts");

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists)
            {
                Log.Info();
                Log.Write("Loading Defs: ");
                Log.Cyan(path);
                Log.Line();
                int countValidFiles = 0;
                int countInvalidFiles = 0;
                foreach (FileInfo fileInfo in dirInfo.GetFiles("*.xml", SearchOption.AllDirectories))
                {
                    XDocument doc = null;
                    try
                    {
                        doc = XDocument.Load(fileInfo.FullName, LoadOptions.SetBaseUri);
                        countValidFiles++;
                    }
                    catch (XmlException ex)
                    {
                        Log.Error();
                        Log.WriteLine(ex.Message);
                        Log.Indent();
                        Log.WriteLine(ex.SourceUri);
                        countInvalidFiles++;
                    }
                    if (doc != null)
                    {
                        this._data.Add(fileInfo.FullName, doc);
                        foreach (XElement abstr in from ele in doc.Root.Elements()
                                                   where ele.Attribute("Name") != null
                                                   select ele)
                        {
                            XElement abstrGroup = this._abstracts.Element(abstr.Name);
                            if (abstrGroup == null)
                            {
                                abstrGroup = new XElement(abstr.Name);
                                abstrGroup.Add(abstr);
                                ((XElement)abstrGroup.LastNode).SetAttributeValue("Uri", abstr.BaseUri);
                                this._abstracts.Add(abstrGroup);
                            }
                            else
                            {
                                abstrGroup.Add(abstr);
                                ((XElement)abstrGroup.LastNode).SetAttributeValue("Uri", abstr.BaseUri);
                            }
                        }
                    }
                }
                if (countValidFiles > 0)
                {
                    Log.Info();
                    Log.WriteLine("Completed Loading Defs: Success: {0} file(s), Failure: {1} file(s).", countValidFiles, countInvalidFiles);
                }
                else if (countInvalidFiles > 0)
                {
                    Log.Error();
                    Log.WriteLine("Loading failed: {1} file(s).", countInvalidFiles);
                }
                else
                {
                    Log.Info();
                    Log.WriteLine("Directory \"Defs\" is empty.");
                }
            }
            else
            {
                Log.Info();
                Log.Write("Directory \"Defs\" does not exist: ");
                Log.Cyan(path);
                Log.Line();
            }
        }

        #endregion

        #region Inherit

        private void Inherit(DefinitionData definitionDataCore = null)
        {
            IEnumerable<XElement> children = from doc in this._data.Values
                                             from ele in doc.Root.Elements()
                                             where ele.Attribute("ParentName") != null && ele.HasField_defName()
                                             select ele;
            if (children.Count() > 0)
            {
                Log.Info();
                Log.WriteLine("Start processing Inheritances.");
                XElement abstracts;
                if (definitionDataCore == null)
                {
                    abstracts = this._abstracts;
                }
                else
                {
                    abstracts = new XElement(this._abstracts);
                    foreach (XElement abstrsCore in definitionDataCore._abstracts.Elements())
                    {
                        XElement abstrs = abstracts.Element(abstrsCore.Name);
                        if (abstrs == null)
                        {
                            abstracts.Add(abstrsCore);
                        }
                        else
                        {
                            abstrs.Add(abstrsCore.Elements());
                        }
                    }
                }
                int countValidChildren = 0;
                int countInvalidChildren = 0;
                foreach (XElement defChild in children)
                {
                    XElement abstrGroup = abstracts.Element(defChild.Name);
                    if (abstrGroup == null)
                    {
                        countInvalidChildren++;
                        Log.Warning();
                        Log.WriteLine("Could not found parent node for node \"{0}\", ParentName=\"{1}\".",
                            defChild.Name.ToString(),
                            defChild.Attribute("ParentName").Value);
                        Log.WriteLine(defChild.BaseUri);
                    }
                    else
                    {
                        bool valid = true;
                        XElement defParent = defChild;
                        do
                        {
                            bool isParentFound = false;
                            string parentName = defParent.Attribute("ParentName").Value;
                            foreach (XElement abstr in abstrGroup.Elements())
                            {
                                if (abstr.Attribute("Name").Value == parentName)
                                {
                                    isParentFound = true;
                                    defParent = abstr;
                                    InheritRecursively(defChild, defParent);
                                    break;
                                }
                            }
                            if (!isParentFound)
                            {
                                valid = false;
                                Log.Warning();
                                Log.WriteLine("Could not found parent node for node \"{0}\", ParentName=\"{1}\".",
                                    defParent.Name.ToString(),
                                    parentName);
                                Log.WriteLine(defParent == defChild ? defChild.BaseUri : defParent.Attribute("Uri").Value);
                                break;
                            }
                        } while (defParent.Attribute("ParentName") != null);
                        if (valid)
                        {
                            countValidChildren++;
                        }
                        else
                        {
                            countInvalidChildren++;
                        }
                    }
                }
                if (countValidChildren > 0)
                {
                    Log.Info();
                    Log.WriteLine("Completed Inheriting: Success: {0} node(s), Failure: {1} node(s).", countValidChildren, countInvalidChildren);
                }
                else
                {
                    Log.Error();
                    Log.WriteLine("Inheriting Failed: {0} node(s).", countInvalidChildren);
                }
            }
        }

        private static void InheritRecursively(XElement child, XElement parent)
        {
            if (child.HasElements && parent.HasElements)
            {
                if (child.Elements().First().Name.ToString() == "li")
                {
                    child.AddFirst(parent.Elements()); // Must use AddFirst();
                }
                else
                {
                    foreach (XElement fieldParent in parent.Elements())
                    {
                        bool isMatched = false;
                        string fieldNameParent = fieldParent.Name.ToString();
                        foreach (XElement fieldChild in child.Elements())
                        {
                            if (string.Compare(fieldNameParent, fieldChild.Name.ToString(), true) == 0)
                            {
                                isMatched = true;
                                InheritRecursively(fieldChild, fieldParent);
                                break;
                            }
                        }
                        if (!isMatched)
                        {
                            child.Add(fieldParent);
                        }
                    }
                }
            }
        }

        #endregion

        #region Complete

        private static void CompleteAndTidy(ref XElement previousField, ref XElement nextField, string nextFieldName, string value, string valueFormat = null)
        {
            if (nextField == null)
            {
                if (valueFormat == null)
                {
                    previousField.AddAfterSelf(new XElement(nextFieldName, value));
                }
                else
                {
                    previousField.AddAfterSelf(new XElement(nextFieldName, string.Format(valueFormat, value)));
                }
                nextField = previousField.ElementsAfterSelf().First();
            }
            else
            {
                try
                {
                    if (nextField != previousField.ElementsAfterSelf().First())
                    {
                        nextField.Remove();
                        previousField.AddAfterSelf(nextField);
                        nextField = previousField.ElementsAfterSelf().First();
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void CompletePawnKindLabel(DefinitionData definitionDataCore = null)
        {
            IEnumerable<XElement> pawnKindDefsAll = from doc in this._data.Values
                                                    from ele in doc.Root.Elements(DefTypeNameOf.PawnKindDef)
                                                    where ele.HasField_defName()
                                                    select ele;
            int countPawnKindDefs = pawnKindDefsAll.Count();
            if (countPawnKindDefs > 0)
            {
                Log.Info();
                Log.WriteLine("Start processing PawnKindDefs.");
                IEnumerable<XElement> racesAll = GetAllRaces(this._data);
                if (definitionDataCore != null)
                {
                    racesAll.Concat(GetAllRaces(definitionDataCore._data));
                }

                foreach (XElement pawnKindDef in pawnKindDefsAll)
                {
                    bool hasGenders = true;
                    bool isHumanlike = false;
                    string raceName = pawnKindDef.Field(FieldNameOf.race).Value;
                    foreach (XElement race in racesAll)
                    {
                        if (race.defName().Value == raceName)
                        {
                            XElement field_race = race.Field(FieldNameOf.race);
                            if (field_race != null)
                            {
                                XElement field_hasGenders = field_race.Field(FieldNameOf.hasGenders);
                                if (field_hasGenders != null && !bool.TryParse(field_hasGenders.Value, out hasGenders))
                                {
                                    hasGenders = true;
                                }
                                XElement field_intelligence = field_race.Field(FieldNameOf.intelligence);
                                if (field_intelligence != null && field_intelligence.Value == "Humanlike")
                                {
                                    isHumanlike = true;
                                }
                            }
                            break;
                        }
                    }


                    // Process root
                    XElement defName = pawnKindDef.defName();

                    // label
                    XElement label = pawnKindDef.Field(FieldNameOf.label);
                    bool flag_Plural = false;
                    XElement labelPlural = pawnKindDef.Field(FieldNameOf.labelPlural);
                    if (labelPlural != null) flag_Plural = true;

                    // Male
                    bool flag_Male = false;
                    XElement labelMale = pawnKindDef.Field(FieldNameOf.labelMale);
                    if (labelMale != null) flag_Male = true;
                    // MalePlural
                    bool flag_MalePlural = false;
                    XElement labelMalePlural = pawnKindDef.Field(FieldNameOf.labelMalePlural);
                    if (labelMalePlural != null) flag_MalePlural = true;

                    // Female
                    bool flag_Female = false;
                    XElement labelFemale = pawnKindDef.Field(FieldNameOf.labelFemale);
                    if (labelFemale != null) flag_Female = true;
                    // FemalePlural
                    bool flag_FemalePlural = false;
                    XElement labelFemalePlural = pawnKindDef.Field(FieldNameOf.labelFemalePlural);
                    if (labelFemalePlural != null) flag_FemalePlural = true;

                    // label
                    CompleteAndTidy(ref defName, ref label, FieldNameOf.label, defName.Value);
                    // Plural
                    CompleteAndTidy(ref label, ref labelPlural, FieldNameOf.labelPlural, label.Value, "{0}s");

                    // Male
                    CompleteAndTidy(ref labelPlural, ref labelMale, FieldNameOf.labelMale, label.Value, "male {0}");
                    // MalePlural
                    if (flag_Plural)
                    {
                        CompleteAndTidy(ref labelMale, ref labelMalePlural, FieldNameOf.labelMalePlural, labelPlural.Value, "male {0}");
                    }
                    else
                    {
                        CompleteAndTidy(ref labelMale, ref labelMalePlural, FieldNameOf.labelMalePlural, labelMale.Value, "{0}s");
                    }

                    // Female
                    CompleteAndTidy(ref labelMalePlural, ref labelFemale, FieldNameOf.labelFemale, label.Value, "female {0}");
                    // FemalePlural
                    if (flag_Plural)
                    {
                        CompleteAndTidy(ref labelFemale, ref labelFemalePlural, FieldNameOf.labelFemalePlural, labelPlural.Value, "female {0}");
                    }
                    else
                    {
                        CompleteAndTidy(ref labelFemale, ref labelFemalePlural, FieldNameOf.labelFemalePlural, labelFemale.Value, "{0}s");
                    }

                    bool flag_NothingInLifeStage = true;

                    // Process lifeStages
                    XElement lifeStages = pawnKindDef.Field(FieldNameOf.lifeStages);
                    if (lifeStages != null && lifeStages.Elements().Count() > 0)
                    {
                        IEnumerable<XElement> lifeStagesList = lifeStages.Elements();
                        // Patch all labels in lifeStages
                        foreach (XElement curLifeStage in lifeStagesList)
                        {
                            // label
                            bool curflag_label = false;
                            XElement curLabel = curLifeStage.Field(FieldNameOf.label);
                            if (curLabel != null) curflag_label = true;
                            bool curflag_Plural = false;
                            XElement curLabelPlural = curLifeStage.Field(FieldNameOf.labelPlural);
                            if (curLabelPlural != null) curflag_Plural = true;

                            // labelMale
                            bool curflag_Male = false;
                            XElement curLabelMale = curLifeStage.Field(FieldNameOf.labelMale);
                            if (curLabelMale != null) curflag_Male = true;
                            bool curflag_MalePlural = false;
                            XElement curLabelMalePlural = curLifeStage.Field(FieldNameOf.labelMalePlural);
                            if (curLabelMalePlural != null) curflag_MalePlural = true;

                            // Female
                            bool curflag_Female = false;
                            XElement curLabelFemale = curLifeStage.Field(FieldNameOf.labelFemale);
                            if (curLabelFemale != null) curflag_Female = true;
                            // FemalPlural
                            bool curflag_FemalePlural = false;
                            XElement curLabelFemalePlural = curLifeStage.Field(FieldNameOf.labelFemalePlural);
                            if (curLabelFemalePlural != null) curflag_FemalePlural = true;

                            ////////////////////////////////////////////////////////////////

                            if (lifeStages.Elements().Count() == 1 && !curflag_label && !curflag_Male && !curflag_Female)
                            {
                                flag_NothingInLifeStage = true;
                                break;
                            }
                            else
                            {
                                flag_NothingInLifeStage = false;
                            }

                            ////////////////////////////////////////////////////////////////

                            // label
                            if (curLabel == null)
                            {
                                curLifeStage.AddFirst(label);
                                curLabel = curLifeStage.Field(FieldNameOf.label);
                            }
                            else if (curLabel != curLifeStage.Elements().First())
                            {
                                curLabel.Remove();
                                curLifeStage.AddFirst(curLabel);
                                curLabel = curLifeStage.Elements().First();
                            }
                            // Plural
                            if (curflag_label)
                            {
                                CompleteAndTidy(ref curLabel, ref curLabelPlural, FieldNameOf.labelPlural, curLabel.Value, "{0}s");
                            }
                            else if (flag_Plural)
                            {
                                CompleteAndTidy(ref curLabel, ref curLabelPlural, FieldNameOf.labelPlural, labelPlural.Value);
                            }
                            else
                            {
                                CompleteAndTidy(ref curLabel, ref curLabelPlural, FieldNameOf.labelPlural, label.Value);
                            }

                            // Male
                            if (curflag_label)
                            {
                                CompleteAndTidy(ref curLabelPlural, ref curLabelMale, FieldNameOf.labelMale, curLabel.Value, "male {0}");
                            }
                            else if (flag_Male)
                            {
                                CompleteAndTidy(ref curLabelPlural, ref curLabelMale, FieldNameOf.labelMale, labelMale.Value);
                            }
                            else
                            {
                                CompleteAndTidy(ref curLabelPlural, ref curLabelMale, FieldNameOf.labelMale, label.Value, "male {0}");
                            }
                            // MalePlural
                            if (curflag_Male)
                            {
                                CompleteAndTidy(ref curLabelMale, ref curLabelMalePlural, FieldNameOf.labelMalePlural, curLabelMale.Value, "{0}s");
                            }
                            else if (curflag_Plural)
                            {
                                CompleteAndTidy(ref curLabelMale, ref curLabelMalePlural, FieldNameOf.labelMalePlural, curLabelPlural.Value, "male {0}");
                            }
                            else if (curflag_label)
                            {
                                CompleteAndTidy(ref curLabelMale, ref curLabelMalePlural, FieldNameOf.labelMalePlural, curLabel.Value, "male {0}s");
                            }
                            else
                            {
                                CompleteAndTidy(ref curLabelMale, ref curLabelMalePlural, FieldNameOf.labelMalePlural, label.Value, "male {0}s");
                            }

                            // Female
                            if (curflag_label)
                            {
                                CompleteAndTidy(ref curLabelMalePlural, ref curLabelFemale, FieldNameOf.labelFemale, curLabel.Value, "female {0}");
                            }
                            else if (flag_Female)
                            {
                                CompleteAndTidy(ref curLabelMalePlural, ref curLabelFemale, FieldNameOf.labelFemale, labelFemale.Value);
                            }
                            else
                            {
                                CompleteAndTidy(ref curLabelMalePlural, ref curLabelFemale, FieldNameOf.labelFemale, label.Value, "female {0}");
                            }
                            // FemalePlural
                            if (curflag_Female)
                            {
                                CompleteAndTidy(ref curLabelFemale, ref curLabelFemalePlural, FieldNameOf.labelFemalePlural, curLabelFemale.Value, "{0}s");
                            }
                            else if (curflag_Plural)
                            {
                                CompleteAndTidy(ref curLabelFemale, ref curLabelFemalePlural, FieldNameOf.labelFemalePlural, curLabelPlural.Value, "female {0}");
                            }
                            else if (curflag_label)
                            {
                                CompleteAndTidy(ref curLabelFemale, ref curLabelFemalePlural, FieldNameOf.labelFemalePlural, curLabel.Value, "female {0}s");
                            }
                            else
                            {
                                CompleteAndTidy(ref curLabelFemale, ref curLabelFemalePlural, FieldNameOf.labelFemalePlural, label.Value, "female {0}s");
                            }

                            // Remove labels according to hasGenders
                            if (hasGenders)
                            {
                                curLabel.Remove();
                                curLabelPlural.Remove();
                            }
                            else
                            {
                                curLabelMale.Remove();
                                curLabelMalePlural.Remove();
                                curLabelFemale.Remove();
                                curLabelFemalePlural.Remove();
                            }
                        }
                    }

                    // Remove root labels
                    if (flag_NothingInLifeStage)
                    {
                        if (isHumanlike)
                        {
                            if ((!flag_Male && !flag_Female) || !hasGenders)
                            {
                                labelMale.Remove();
                                labelMalePlural.Remove();
                                labelFemale.Remove();
                                labelFemalePlural.Remove();
                            }
                        }
                        else
                        {
                            if (!hasGenders)
                            {
                                labelMale.Remove();
                                labelMalePlural.Remove();
                                labelFemale.Remove();
                                labelFemalePlural.Remove();
                            }
                        }
                    }
                    else
                    {
                        //if(!flag_Plural) labelPlural.Remove();
                        labelMale.Remove();
                        labelMalePlural.Remove();
                        labelFemale.Remove();
                        labelFemalePlural.Remove();
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing PawnKindDefs: {0} node(s).", countPawnKindDefs);
            }
        }

        public static IEnumerable<XElement> GetAllRaces(SortedDictionary<string, XDocument> data)
        {
            foreach (XElement thingDef in from doc in data.Values
                                          from ele in doc.Root.Elements(DefTypeNameOf.ThingDef)
                                          where ele.HasField_defName()
                                          select ele)
            {
                XElement category = thingDef.Field(FieldNameOf.category);
                if (category != null && category.Value == "Pawn")
                {
                    yield return thingDef;
                }
            }
        }

        private void CompletePawnRelationLabel()
        {
            IEnumerable<XElement> pawnRelationDefsAll = from doc in this._data.Values
                                                        from ele in doc.Root.Elements(DefTypeNameOf.PawnRelationDef)
                                                        where ele.HasField_defName()
                                                        select ele;
            int countPawnRelationDefs = pawnRelationDefsAll.Count();
            if (countPawnRelationDefs > 0)
            {
                Log.Info();
                Log.WriteLine("Start processing PawnRelationDefs.");
                foreach (XElement pawnRelationDef in pawnRelationDefsAll)
                {
                    XElement defName = pawnRelationDef.defName();
                    XElement label = pawnRelationDef.Field(FieldNameOf.label);
                    CompleteAndTidy(ref defName, ref label, FieldNameOf.label, defName.Value);
                    XElement labelFemale = pawnRelationDef.Field(FieldNameOf.labelFemale);
                    CompleteAndTidy(ref label, ref labelFemale, FieldNameOf.labelFemale, label.Value);
                }
                Log.Info();
                Log.WriteLine("Completed processing PawnRelationDefs: {0} node(s).", countPawnRelationDefs);
            }
        }

        private void CompleteScenarioNameAndDesc()
        {
            IEnumerable<XElement> scenarioDefsAll = from doc in this._data.Values
                                                    from ele in doc.Root.Elements(DefTypeNameOf.ScenarioDef)
                                                    where ele.HasField_defName()
                                                    select ele;
            int countScenarioDefs = scenarioDefsAll.Count();
            if (countScenarioDefs > 0)
            {
                Log.Info();
                Log.WriteLine("Start processing ScenarioDefs.");
                foreach (XElement scenarioDef in scenarioDefsAll)
                {
                    XElement defName = scenarioDef.defName();
                    XElement label = scenarioDef.Field(FieldNameOf.label);
                    CompleteAndTidy(ref defName, ref label, FieldNameOf.label, defName.Value);
                    XElement description = scenarioDef.Field(FieldNameOf.description);
                    CompleteAndTidy(ref label, ref description, FieldNameOf.description, label.Value);
                    XElement scenario = scenarioDef.Field(FieldNameOf.scenario);
                    if (scenario != null)
                    {
                        XElement name = scenario.Field(FieldNameOf.name);
                        if (name == null)
                        {
                            scenario.AddFirst(new XElement(FieldNameOf.name, label.Value));
                            name = scenario.Field(FieldNameOf.name);
                        }
                        XElement desc = scenario.Field(FieldNameOf.description);
                        if (desc == null)
                        {
                            name.AddAfterSelf(new XElement(FieldNameOf.description, description.Value));
                        }
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing ScenarioDefs: {0} node(s).", countScenarioDefs);
            }
        }

        private void CompleteSkillLabel()
        {
            IEnumerable<XElement> skillDefsAll = from doc in this._data.Values
                                                    from ele in doc.Root.Elements(DefTypeNameOf.SkillDef)
                                                    where ele.HasField_defName()
                                                    select ele;
            int countSkillDefs = skillDefsAll.Count();
            if (countSkillDefs > 0)
            {
                Log.Info();
                Log.WriteLine("Start processing SkillDefs.");
                foreach (XElement skillDef in skillDefsAll)
                {
                    XElement defName = skillDef.defName();
                    XElement label = skillDef.Field(FieldNameOf.label);
                    XElement skillLabel = skillDef.Field(FieldNameOf.skillLabel);
                    if (label == null)
                    {
                        if (skillLabel == null)
                        {
                            defName.AddAfterSelf(new XElement(FieldNameOf.label, defName.Value));
                        }
                        else
                        {
                            defName.AddAfterSelf(new XElement(FieldNameOf.label, skillLabel.Value));
                        }
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing SkillDefs: {0} node(s).", countSkillDefs);
            }
        }

        private void CompleteThingDefStuffAdjective()
        {
            IEnumerable<XElement> stuffsAll = GetAllStuffs(this._data);
            int countStuffs = stuffsAll.Count();
            if (countStuffs > 0)
            {
                Log.Info();
                Log.WriteLine("Start processing StuffAdjective.");
                foreach (XElement stuff in stuffsAll)
                {
                    XElement defName = stuff.defName();
                    XElement label = stuff.Field(FieldNameOf.label);
                    CompleteAndTidy(ref defName, ref label, FieldNameOf.label, defName.Value);
                    XElement stuffProps = stuff.Field(FieldNameOf.stuffProps);
                    XElement stuffAdjective = stuffProps.Field(FieldNameOf.stuffAdjective);
                    if (stuffAdjective == null)
                    {
                        stuffProps.AddFirst(new XElement(FieldNameOf.stuffAdjective, label.Value));
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing StuffAdjective: {0} node(s).", countStuffs);
            }
        }

        public static IEnumerable<XElement> GetAllStuffs(SortedDictionary<string, XDocument> data)
        {
            foreach (XElement thingDef in from doc in data.Values
                                          from ele in doc.Root.Elements(DefTypeNameOf.ThingDef)
                                          where ele.HasField_defName()
                                          select ele)
            {
                XElement category = thingDef.Field(FieldNameOf.category);
                XElement stuffProps = thingDef.Field(FieldNameOf.stuffProps);
                if (category != null && category.Value == "Item" && stuffProps != null)
                {
                    yield return thingDef;
                }
            }
        }

        private void CompleteWorkTypeLabel()
        {
            IEnumerable<XElement> workTypeDefsAll = from doc in this._data.Values
                                                    from ele in doc.Root.Elements(DefTypeNameOf.WorkTypeDef)
                                                    where ele.HasField_defName()
                                                    select ele;
            int countWorkTypeDefs = workTypeDefsAll.Count();
            if (countWorkTypeDefs > 0)
            {
                Log.Info();
                Log.WriteLine("Start processing WorkTypeDefs.");
                foreach (XElement workTypeDef in workTypeDefsAll)
                {
                    XElement defName = workTypeDef.defName();
                    XElement label = workTypeDef.Field(FieldNameOf.label);
                    XElement labelShort = workTypeDef.Field(FieldNameOf.labelShort);
                    if (label == null)
                    {
                        if (labelShort == null)
                        {
                            defName.AddAfterSelf(new XElement(FieldNameOf.label, defName.Value));
                        }
                        else
                        {
                            defName.AddAfterSelf(new XElement(FieldNameOf.label, labelShort.Value));
                        }
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing WorkTypeDefs: {0} node(s).", countWorkTypeDefs);
            }
        }

        #endregion

        #region Handle Details

        private void HandleDetails()
        {
            IEnumerable<XElement> defsAll = from doc in this._data.Values
                                            from ele in doc.Root.Elements()
                                            where ele.HasField_defName()
                                            select ele;
            int countDefs = defsAll.Count();
            if (countDefs > 0)
            {
                Log.Info();
                Log.WriteLine("Start processing other details.");
                foreach (XElement def in defsAll)
                {
                    XElement defName = def.defName();
                    XElement firstField = def.Elements().First();
                    if (defName != firstField)
                    {
                        defName.Remove();
                        def.AddFirst(defName);
                        defName = def.defName();
                    }
                    XElement label = def.Field(FieldNameOf.label);
                    if (label != null && label != defName.ElementsAfterSelf().First())
                    {
                        label.Remove();
                        defName.AddAfterSelf(label);
                        label = def.Field(FieldNameOf.label);
                    }
                    XElement description = def.Field(FieldNameOf.description);
                    if (description != null)
                    {
                        if (label == null)
                        {
                            if (description != defName.ElementsAfterSelf().First())
                            {
                                description.Remove();
                                defName.AddAfterSelf(description);
                            }
                        }
                        else
                        {
                            if (description != label.ElementsAfterSelf().First())
                            {
                                description.Remove();
                                label.AddAfterSelf(description);
                            }
                        }
                    }
                    MarkIndexRecursively(def);
                }
                Log.Info();
                Log.WriteLine("Completed processing details.");
            }
        }

        private static void MarkIndexRecursively(XElement ele)
        {
            if (ele.HasElements)
            {
                IEnumerable<XElement> children = ele.Elements();
                IEnumerable<XElement> listItems = ele.Elements("li");
                if (children.Count() == listItems.Count())
                {
                    int index = 0;
                    foreach (XElement child in children)
                    {
                        child.SetAttributeValue("ListIndex", index);
                        index++;
                        MarkIndexRecursively(child);
                    }
                }
                else
                {
                    foreach (XElement child in ele.Elements())
                    {
                        MarkIndexRecursively(child);
                    }
                }
            }
        }

        #endregion

        #region Debug

        public void Debug(string filePath)
        {
            XDocument doc;
            if (this._data.TryGetValue(filePath, out doc))
            {
                Log.Cyan(filePath);
                Log.WriteLine(doc.ToString());
            }
        }

        public void Debug()
        {
            //Debug(@"D:\Game\RimWorld\Mods\Core\Defs\ThingDefs_Races\Races_Animal_Arid.xml");
            //Debug(@"D:\Game\RimWorld\Mods\Core\Defs\PawnKindDefs_Humanlikes\PawnKinds_Mechanoid.xml");
            //Debug(@"D:\Game\RimWorld\Mods\Core\Defs\PawnKindDefs_Humanlikes\PawnKinds_Mercenary.xml");
            //Debug(@"D:\Game\RimWorld\Mods\Core\Defs\PawnRelationDefs\PawnRelations_FamilyByBlood.xml");
            //Debug(@"D:\Game\RimWorld\Mods\Core\Defs\Scenarios\Scenarios_Classic.xml");
            //Debug(@"D:\Game\RimWorld\Mods\Core\Defs\SkillDefs\Skills.xml");
            //Debug(@"D:\Game\RimWorld\Mods\Core\Defs\ThingDefs_Items\Items_Resource_Stuff.xml");
            //Debug(@"D:\Game\RimWorld\Mods\Core\Defs\HediffDefs\Hediffs_Global_Temperature.xml");
            //Debug(@"D:\Game\RimWorld\Mods\Core\Defs\RulePackDefs\RulePacks_NameMaker_World.xml");
            //Debug(@"D:\Game\RimWorld\Mods\Core\Defs\ThingDefs_Misc\Weapons_Grenades.xml");
        }

        #endregion
    }
}
