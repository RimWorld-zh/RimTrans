using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RimTransLib
{
    public class DefinitionData
    {
        private DefinitionData(DirectoryInfo definitionDir)
        {
            this._rootDir = definitionDir;
        }

        #region Info

        private DirectoryInfo _rootDir;
        public DirectoryInfo RootDir
        {
            get
            {
                return _rootDir;
            }
        }

        #endregion

        #region Data

        private SortedDictionary<string, XDocument> _dataBase;
        public SortedDictionary<string, XDocument> DataBase
        {
            get
            {
                return _dataBase;
            }
        }

        private XElement _abstraction;
        public XElement Abstraction
        {
            get
            {
                return _abstraction;
            }
        }

        private XElement _gendersSheet;
        public XElement GendersSheet
        {
            get
            {
                return _gendersSheet;
            }
        }

        #endregion

        #region Loader

        public static DefinitionData Load(DirectoryInfo definitionDir, DefinitionData definitionCore = null)
        {
            DefinitionData definition = new DefinitionData(definitionDir);

            definition._dataBase = new SortedDictionary<string, XDocument>();
            definition._abstraction = new XElement("Abstracts");
            if (definition._rootDir.Exists)
            {
                foreach (FileInfo fileInfo in definition._rootDir.GetFiles("*.xml", SearchOption.AllDirectories))
                {
                    try
                    {
                        XDocument doc = XDocument.Load(fileInfo.FullName, LoadOptions.SetBaseUri);
                        definition._dataBase.Add(fileInfo.FullName, doc);
                        definition._abstraction.Add(from ele in doc.Root.Elements()
                                                    where ele.Attribute("Name") != null
                                                    select ele);
                    }
                    catch (XmlException ex)
                    {
                        //TODO: log
                    }
                }
            }
            if (definitionCore != null)
            {
                definition._abstraction.Add(definitionCore._abstraction.Elements());
            }
            definition.Inherit();

            definition._gendersSheet = new XElement("GendersSheet");
            foreach (XElement thingDefRace in from doc in definition._dataBase.Values
                                              from ele in doc.Root.Elements(RWDefType.ThingDef.ToString())
                                              where RWXml.GetCategory(ele) == RWThingCategory.Pawn && RWXml.HasDefName(ele)
                                              select ele)
            {
                XElement defName = RWXml.GetField(thingDefRace, RWFieldName.defName);
                XElement race = RWXml.GetField(thingDefRace, RWFieldName.race);
                if (race != null)
                {
                    XElement hasGenders = RWXml.GetField(race, RWFieldName.hasGenders);
                    if (hasGenders != null)
                    {
                        definition._gendersSheet.Add(new XElement(defName.Value, hasGenders.Value));
                    }
                }
            }
            if (definitionCore != null)
            {
                definition._gendersSheet.Add(definitionCore._gendersSheet.Elements());
            }
            definition.CompletePawnKindLabel();
            definition.CompletePawnRelationshipLabel();
            definition.CompleteScenarioNameAndDesc();
            definition.CompleteSkillLabel();
            definition.CompleteStuffAdjective();
            definition.MarkListItemIndex();

            return definition;
        }

        #endregion

        #region Abstraction and Inheritance

        private void Inherit()
        {
            foreach (XElement defChild in from doc in this._dataBase.Values
                                     from ele in doc.Root.Elements()
                                     where ele.Attribute("ParentName") != null && RWXml.HasDefName(ele)
                                     select ele)
            {
                XElement defParent = defChild;
                do
                {
                    bool isParentFound = false;
                    foreach (XElement defAbstract in from ele in this._abstraction.Elements()
                                               where ele.Name == defParent.Name && ele.Attribute("Name").Value == defParent.Attribute("ParentName").Value
                                                     select ele)
                    {
                        isParentFound = true;
                        defParent = defAbstract;
                        InheritRecursively(defChild, defParent);  //recursively
                        break;
                    }
                    if (!isParentFound)
                    {
                        //TODO: Log
                        break;
                    }
                } while (defParent.Attribute("ParentName") != null);
            }
        }

        private static void InheritRecursively(XElement child, XElement parent)
        {
            if (child.HasElements && parent.HasElements)
            {
                if (RWXml.GetFieldName(parent.Elements().First()) == RWFieldName.li)
                {
                    child.AddFirst(parent.Elements());  //Must use AddFirst()
                }
                else
                {
                    foreach (XElement ele_parent in parent.Elements())
                    {
                        bool isOverwrited = false;
                        foreach (XElement ele_child in child.Elements())
                        {
                            if (string.Compare(ele_child.Name.ToString(), ele_parent.Name.ToString(), true) == 0)
                            {
                                isOverwrited = true;
                                InheritRecursively(ele_child, ele_parent);  //recursively
                                break;
                            }
                        }
                        if (!isOverwrited)
                        {
                            child.Add(ele_parent);
                        }
                    }
                }
            }
        }

        #endregion

        #region Completion and Marker

        private void CompletePawnKindLabel()
        {
            //TODO: labelPlural
            
            // Patch PawnKindDef
            foreach (XElement pawnKindDef in from doc in this._dataBase.Values
                                             from ele in doc.Root.Elements(RWDefType.PawnKindDef.ToString())
                                             where RWXml.HasDefName(ele)
                                             select ele)
            {
                XElement defName = RWXml.GetField(pawnKindDef, RWFieldName.defName);
                XElement rootLabel = RWXml.GetField(pawnKindDef, RWFieldName.label);
                if (rootLabel == null)
                {
                    defName.AddAfterSelf(new XElement(RWFieldName.label.ToString(), defName.Value));
                    rootLabel = RWXml.GetField(pawnKindDef, RWFieldName.label);
                }
                XElement rootLabelMale = RWXml.GetField(pawnKindDef, RWFieldName.labelMale);
                XElement rootLabelFemale = RWXml.GetField(pawnKindDef, RWFieldName.labelFemale);
                XElement rootLifeStages = RWXml.GetField(pawnKindDef, RWFieldName.lifeStages);
                if (rootLifeStages != null && rootLifeStages.Elements().Count() > 1) // Issue: "> 1" or "> 2"?
                {
                    IEnumerable<XElement> lifeStages = rootLifeStages.Elements();
                    // Patch all of the labels
                    foreach (XElement li in lifeStages)
                    {
                        XElement label = RWXml.GetField(li, RWFieldName.label);
                        if (label == null)
                        {
                            li.AddFirst(rootLabel);
                            label = RWXml.GetField(li, RWFieldName.label);
                        }
                        XElement labelMale = RWXml.GetField(li, RWFieldName.labelMale);
                        if (labelMale == null)
                        {
                            if (rootLabelMale == null)
                            {
                                label.AddAfterSelf(new XElement(RWFieldName.labelMale.ToString(), rootLabel.Value));
                            }
                            else
                            {
                                label.AddAfterSelf(rootLabelMale);
                            }
                            labelMale = RWXml.GetField(li, RWFieldName.labelMale);
                        }
                        else
                        {
                            if (!labelMale.IsAfter(label))
                            {
                                labelMale.Remove();
                                label.AddAfterSelf(labelMale);
                                labelMale = RWXml.GetField(li, RWFieldName.labelMale);
                            }
                        }
                        XElement labelFemale = RWXml.GetField(li, RWFieldName.labelFemale);
                        if (labelFemale == null)
                        {
                            if (rootLabelFemale == null)
                            {
                                labelMale.AddAfterSelf(new XElement(RWFieldName.labelFemale.ToString(), rootLabel.Value));
                            }
                            else
                            {
                                labelMale.AddAfterSelf(rootLabelFemale);
                            }
                        }
                        else
                        {
                            if (!labelFemale.IsAfter(labelMale))
                            {
                                labelFemale.Remove();
                                labelMale.AddAfterSelf(labelFemale);
                                labelFemale = RWXml.GetField(li, RWFieldName.labelFemale);
                            }
                        }
                    }
                    // Get the genders info
                    bool hasGenders = true;
                    XElement race = RWXml.GetField(pawnKindDef, RWFieldName.race);
                    if (race != null)
                    {
                        XElement pawnHasGenders = this._gendersSheet.Element(race.Value);
                        hasGenders = (string.Compare(pawnHasGenders.Value, "true", true) == 0);
                    }
                    else
                    {
                        hasGenders = true;
                    }
                    // Remove labels according to the genders info 
                    foreach (XElement li in lifeStages)
                    {
                        XElement label = RWXml.GetField(li, RWFieldName.label);
                        XElement labelMale = RWXml.GetField(li, RWFieldName.labelMale);
                        XElement labelFemale = RWXml.GetField(li, RWFieldName.labelFemale);
                        if (hasGenders)
                        {
                            label.Remove();
                        }
                        else
                        {
                            labelMale.Remove();
                            labelFemale.Remove();
                        }
                    }
                    if (rootLabelMale != null) rootLabelMale.Remove();
                    if (rootLabelFemale != null) rootLabelFemale.Remove();
                }
            }
        }

        private void CompletePawnRelationshipLabel()
        {
            foreach (XElement pawnRelationDef in from doc in this._dataBase.Values
                                                 from ele in doc.Root.Elements(RWDefType.PawnRelationDef.ToString())
                                                 where RWXml.HasDefName(ele)
                                                 select ele)
            {
                XElement defName = RWXml.GetField(pawnRelationDef, RWFieldName.defName);
                XElement label = RWXml.GetField(pawnRelationDef, RWFieldName.label);
                if (label == null)
                {
                    defName.AddAfterSelf(new XElement(RWFieldName.label.ToString(), defName.Value));
                    label = RWXml.GetField(pawnRelationDef, RWFieldName.label);
                }
                XElement labelFemale = RWXml.GetField(pawnRelationDef, RWFieldName.labelFemale);
                if (labelFemale == null)
                {
                    label.AddAfterSelf(new XElement(RWFieldName.labelFemale.ToString(), label.Value));
                    //labelFemale = RWXml.GetField(pawnRelationDef, RWFieldName.labelFemale);
                }
            }
        }

        private void CompleteScenarioNameAndDesc()
        {
            foreach (XElement scenarioDef in from doc in this._dataBase.Values
                                             from ele in doc.Root.Elements(RWDefType.ScenarioDef.ToString())
                                             where RWXml.HasDefName(ele)
                                             select ele)
            {
                XElement defName = RWXml.GetField(scenarioDef, RWFieldName.defName);
                XElement rootLabel = RWXml.GetField(scenarioDef, RWFieldName.label);
                if (rootLabel == null)
                {
                    defName.AddAfterSelf(new XElement(RWFieldName.label.ToString(), defName.Value));
                    rootLabel = RWXml.GetField(scenarioDef, RWFieldName.label);
                }
                XElement rootDescription = RWXml.GetField(scenarioDef, RWFieldName.description);
                if (rootDescription == null)
                {
                    rootLabel.AddAfterSelf(new XElement(RWFieldName.description.ToString(), rootLabel.Value));
                    rootDescription = RWXml.GetField(scenarioDef, RWFieldName.description);
                }
                XElement scenario = RWXml.GetField(scenarioDef, RWFieldName.scenario);
                if (scenario != null)
                {
                    XElement name = RWXml.GetField(scenario, RWFieldName.name);
                    if (name == null)
                    {
                        scenario.AddFirst(new XElement(RWFieldName.name.ToString(), rootLabel.Value));
                        name = RWXml.GetField(scenario, RWFieldName.name);
                    }
                    XElement description = RWXml.GetField(scenario, RWFieldName.description);
                    if (description == null)
                    {
                        name.AddAfterSelf(new XElement(RWFieldName.description.ToString(), rootDescription.Value));
                        //description = RWXml.GetField(scenario, RWFieldName.description);
                    }
                }
            }
        }

        private void CompleteSkillLabel()
        {
            foreach (XElement skillDef in from doc in this._dataBase.Values
                                          from ele in doc.Root.Elements(RWDefType.SkillDef.ToString())
                                          where RWXml.HasDefName(ele)
                                          select ele)
            {
                XElement defName = RWXml.GetField(skillDef, RWFieldName.defName);
                XElement label = RWXml.GetField(skillDef, RWFieldName.label);
                XElement skillLabel = RWXml.GetField(skillDef, RWFieldName.skillLabel);
                if (label == null)
                {
                    if (skillLabel == null)
                    {
                        defName.AddAfterSelf(new XElement(RWFieldName.label.ToString(), defName.Value));
                    }
                    else
                    {
                        defName.AddAfterSelf(new XElement(RWFieldName.label.ToString(), skillLabel.Value));
                    }
                }
            }
        }

        private void CompleteStuffAdjective()
        {
            foreach (XElement thingDefStuff in from doc in this._dataBase.Values
                                               from ele in doc.Root.Elements(RWDefType.ThingDef.ToString())
                                               where RWXml.HasDefName(ele)
                                               select ele)
            {
                XElement defName = RWXml.GetField(thingDefStuff, RWFieldName.defName);
                XElement label = RWXml.GetField(thingDefStuff, RWFieldName.label);
                if (label == null)
                {
                    defName.AddAfterSelf(new XElement(RWFieldName.label.ToString(), defName.Value));
                    label = RWXml.GetField(thingDefStuff, RWFieldName.label);
                }
                XElement stuffProps = RWXml.GetField(thingDefStuff, RWFieldName.stuffProps);
                if (stuffProps != null &&
                    RWXml.GetField(stuffProps, RWFieldName.categories) != null &&
                    RWXml.GetField(stuffProps, RWFieldName.stuffAdjective) == null)
                {
                    stuffProps.AddFirst(new XElement(RWFieldName.stuffAdjective.ToString(), label.Value));
                }
            }
        }

        private void MarkListItemIndex()
        {
            foreach (XElement def in from doc in this._dataBase.Values
                                     from ele in doc.Root.Elements()
                                     where RWXml.HasDefName(ele)
                                     select ele)
            {
                foreach (XElement ele in def.Elements())
                {
                    MarkRecursively(ele);
                }
            }
        }

        private static void MarkRecursively(XElement element)
        {
            if (element.HasElements)
            {
                if (element.Element("li") == null)
                {
                    foreach (XElement child in element.Elements())
                    {
                        MarkRecursively(child);
                    }
                }
                else
                {
                    int index = 0;
                    foreach (XElement li in element.Elements("li"))
                    {
                        li.SetAttributeValue("RimTransIndex", index);
                        index++;
                    }
                }
            }
        }

        #endregion

        #region Debug

        public void Display(string filePath)
        {
            XDocument doc;
            if (this._dataBase.TryGetValue(filePath, out doc))
            {
                Console.WriteLine(doc);
            }
            else
            {
                Console.WriteLine("No found: " + filePath);
            }
        }

        #endregion
    }
}
