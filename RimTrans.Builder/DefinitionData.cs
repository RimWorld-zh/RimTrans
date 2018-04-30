using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RimTrans.Builder.Xml;

namespace RimTrans.Builder {
    public class DefinitionData {
        private SortedDictionary<string, XDocument> _data;
        public SortedDictionary<string, XDocument> Data { get { return this._data; } }

        // Storage nodes those have attribute Name="XXXX"
        private XElement _abstracts;

        private bool isProcessedFieldNames = false;
        public bool IsProcessedFieldNames { get { return this.isProcessedFieldNames; } }
        public void MarkProcessedFieldNames() {
            this.isProcessedFieldNames = true;
        }


        #region Methods



        #endregion


        private DefinitionData() {

        }

        public DefinitionData(DefinitionData other) {
            this._data = new SortedDictionary<string, XDocument>();
            foreach (var kvpOther in other._data) {
                this._data.Add(kvpOther.Key, new XDocument(kvpOther.Value));
            }
        }

        #region Load

        /// <summary>
        /// Load from files
        /// </summary>
        /// <param name="definitionDataCore">For getting Core abstractions</param>
        public static DefinitionData Load(string path, DefinitionData definitionDataCore = null) {
            DefinitionData definitionData = new DefinitionData();

            definitionData.Load(path);

            if (definitionData._data.Count == 0)
                return definitionData;

            definitionData.Inherit(definitionDataCore);

            definitionData.MarkIndex();

            definitionData.CompleteDamage();
            definitionData.CompleteJobReportString();
            definitionData.CompletePawnKindLabel(definitionDataCore);
            definitionData.CompletePawnRelationLabel();
            definitionData.CompleteResearchTab();
            definitionData.CompleteScenarioNameAndDesc();
            definitionData.CompleteSkillLabel();
            definitionData.CompleteThingDefStuffAdjective();
            definitionData.CompleteWorkTypeLabel();

            return definitionData;
        }

        private void Load(string path) {
            this._data = new SortedDictionary<string, XDocument>();
            this._abstracts = new XElement("Abstracts");

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists) {
                Log.Info();
                Log.Write("Loading Defs: ");
                Log.WriteLine(ConsoleColor.Cyan, path);
                int countValidFiles = 0;
                int countInvalidFiles = 0;
                int splitIndex = dirInfo.FullName.Length + 1;
                foreach (FileInfo fileInfo in dirInfo.GetFiles("*.xml", SearchOption.AllDirectories)) {
                    XDocument doc = null;
                    string filePath = fileInfo.FullName;
                    try {
                        doc = XDocument.Load(filePath, LoadOptions.SetBaseUri);
                        countValidFiles++;
                    } catch (XmlException ex) {
                        Log.Error();
                        Log.Write("Loading file failed: ");
                        Log.WriteLine(ConsoleColor.Red, filePath);
                        Log.Indent();
                        Log.WriteLine(ex.Message);
                        countInvalidFiles++;
                    }
                    if (doc != null) {
                        foreach (XElement def in doc.Root.Elements()) {
                            foreach (string defTypeName in DefTypeNameOf.AllNames) {
                                if (string.Compare(def.Name.ToString(), defTypeName, true) == 0 && def.Name.ToString() != defTypeName) {
                                    def.Name = defTypeName;
                                }
                            }
                        }
                        this._data.Add(filePath.Substring(splitIndex), doc);
                        foreach (XElement abstr in from ele in doc.Root.Elements()
                                                   where ele.Attribute("Name") != null
                                                   select ele) {
                            XElement abstrGroup = this._abstracts.Element(abstr.Name);
                            if (abstrGroup == null) {
                                abstrGroup = new XElement(abstr.Name);
                                abstrGroup.Add(abstr);
                                ((XElement)abstrGroup.LastNode).SetAttributeValue("Uri", abstr.BaseUri);
                                this._abstracts.Add(abstrGroup);
                            } else {
                                abstrGroup.Add(abstr);
                                ((XElement)abstrGroup.LastNode).SetAttributeValue("Uri", abstr.BaseUri);
                            }
                        }
                    }
                }
                if (countValidFiles > 0) {
                    if (countInvalidFiles == 0) {
                        Log.Info();
                        Log.WriteLine("Completed Loading Defs: {0} file(s).", countValidFiles);
                    } else {
                        Log.Warning();
                        Log.WriteLine("Completed Loading Defs: Success: {0} file(s), Failure: {1} file(s).", countValidFiles, countInvalidFiles);
                    }
                } else {
                    if (countInvalidFiles == 0) {
                        Log.Info();
                        Log.WriteLine("Directory \"Defs\" is empty.");
                    } else {
                        Log.Error();
                        Log.WriteLine("Loading failed: {1} file(s).", countInvalidFiles);
                    }
                }
            } else {
                Log.Info();
                Log.Write("Directory \"Defs\" does not exist: ");
                Log.WriteLine(ConsoleColor.Cyan, path);
            }
        }

        #endregion

        #region Inherit

        /// <summary>
        /// Process Abstractions and Inheritances
        /// </summary>
        private void Inherit(DefinitionData definitionDataCore = null) {
            IEnumerable<XElement> children = from doc in this._data.Values
                                             from ele in doc.Root.Elements()
                                             where ele.Attribute("ParentName") != null && ele.HasField_defName()
                                             select ele;
            if (children.Count() > 0) {
                Log.Info();
                Log.WriteLine("Start processing Inheritances.");
                XElement abstracts;
                if (definitionDataCore == null) {
                    abstracts = this._abstracts;
                } else {
                    abstracts = new XElement(this._abstracts);
                    foreach (XElement abstrsCore in definitionDataCore._abstracts.Elements()) {
                        XElement abstrs = abstracts.Element(abstrsCore.Name);
                        if (abstrs == null) {
                            abstracts.Add(abstrsCore);
                        } else {
                            abstrs.Add(abstrsCore.Elements());
                        }
                    }
                }
                int countValidChildren = 0;
                int countInvalidChildren = 0;
                foreach (XElement defChild in children) {
                    XElement abstrGroup = abstracts.Element(defChild.Name);
                    if (abstrGroup == null) {
                        Log.Warning();
                        Log.Write("Could not found parent for node: ");
                        Log.WriteLine(ConsoleColor.Yellow, "<{0}> defName=\"{1}\" ParentName=\"{2}\"",
                            defChild.Name.ToString(),
                            defChild.defName().Value,
                            defChild.Attribute("ParentName").Value);
                        Log.Indent();
                        Log.WriteLine(ConsoleColor.Yellow, defChild.BaseUri);
                        countInvalidChildren++;
                    } else {
                        bool isFailed = true;
                        XElement defParent = defChild;
                        do {
                            bool isParentFound = false;
                            string parentName = defParent.Attribute("ParentName").Value;
                            foreach (XElement abstr in abstrGroup.Elements()) {
                                if (abstr.Attribute("Name").Value == parentName) {
                                    isParentFound = true;
                                    defParent = abstr;
                                    InheritRecursively(defChild, defParent); // Recursively
                                    break;
                                }
                            }
                            if (!isParentFound) {
                                isFailed = false;
                                if (defParent == defChild) {
                                    Log.Warning();
                                    Log.Write("Could not found parent for node: ");
                                    Log.WriteLine(ConsoleColor.Yellow, "<{0}> defName=\"{1}\" ParentName=\"{2}\"",
                                        defChild.Name.ToString(),
                                        defChild.defName().Value,
                                        parentName);
                                    Log.Indent();
                                    Log.WriteLine(ConsoleColor.Yellow, defChild.BaseUri);
                                } else {
                                    Log.Warning();
                                    Log.Write("Could not found parent for node: ");
                                    Log.WriteLine(ConsoleColor.Yellow, "<{0}> Name=\"{1}\" ParentName=\"{2}\"",
                                        defParent.Name.ToString(),
                                        defParent.Attribute("Name").Value,
                                        parentName);
                                    Log.Indent();
                                    Log.WriteLine(ConsoleColor.Yellow, defParent.Attribute("Uri").Value);
                                }
                                break;
                            }
                        } while (defParent.Attribute("ParentName") != null);
                        if (isFailed) {
                            countValidChildren++;
                        } else {
                            countInvalidChildren++;
                        }
                    }
                }
                if (countValidChildren > 0) {
                    if (countInvalidChildren == 0) {
                        Log.Info();
                        Log.WriteLine("Completed Inheriting: {0} node(s).", countValidChildren);
                    } else {
                        Log.Warning();
                        Log.WriteLine("Completed Inheriting: Success: {0} node(s), Failure: {1} node(s).", countValidChildren, countInvalidChildren);
                    }
                } else {
                    if (countInvalidChildren == 0) {
                        Log.Info();
                        Log.WriteLine("No node need to inherit.");
                    } else {
                        Log.Error();
                        Log.WriteLine("Inheriting Failed: {0} node(s).", countInvalidChildren);
                    }
                }
            }
        }

        /// <summary>
        /// Inherit Recursively
        /// </summary>
        private static void InheritRecursively(XElement child, XElement parent) {
            if (child.HasElements && parent.HasElements) {
                if (child.Elements().First().Name.ToString() == "li" &&
                    parent.Elements().First().Name.ToString() == "li") {
                    child.AddFirst(parent.Elements()); // Must use AddFirst();
                } else {
                    foreach (XElement fieldParent in parent.Elements()) {
                        bool isMatched = false;
                        foreach (XElement fieldChild in child.Elements()) {
                            if (string.Compare(fieldParent.Name.ToString(), fieldChild.Name.ToString(), true) == 0) {
                                isMatched = true;
                                InheritRecursively(fieldChild, fieldParent);
                                break;
                            }
                        }
                        if (!isMatched) {
                            child.Add(fieldParent);
                        }
                    }
                }
            }
        }

        #endregion

        #region Complete

        private static void CompleteAndTidy(ref XElement previousField, ref XElement nextField, string nextFieldName, string value, string valueFormat = null) {
            if (nextField == null) {
                if (valueFormat == null) {
                    previousField.AddAfterSelf(new XElement(nextFieldName, value));
                } else {
                    previousField.AddAfterSelf(new XElement(nextFieldName, string.Format(valueFormat, value)));
                }
                nextField = previousField.ElementsAfterSelf().First();
            } else {
                try {
                    if (nextField != previousField.ElementsAfterSelf().First()) {
                        nextField.Remove();
                        previousField.AddAfterSelf(nextField);
                        nextField = previousField.ElementsAfterSelf().First();
                    }
                } catch (Exception) {
                }
            }
        }

        private void CompleteDamage() {
            IEnumerable<XElement> damageDefsAll = from doc in this._data.Values
                                                  from ele in doc.Root.Elements(DefTypeNameOf.DamageDef)
                                                  where ele.HasField_defName()
                                                  select ele;
            int countDamageDefs = damageDefsAll.Count();
            if (countDamageDefs > 0) {
                Log.Info();
                Log.WriteLine("Start processing DamageDefs.");
                foreach (XElement damageDef in damageDefsAll) {
                    XElement deathMessage = damageDef.Field(FieldNameOf.deathMessage);
                    if (deathMessage == null) {
                        damageDef.Add(new XElement(FieldNameOf.deathMessage, "{0} has been killed."));
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing DamageDefs: {0} node(s).", countDamageDefs);
            }
        }

        private void CompleteJobReportString() {
            IEnumerable<XElement> jobDefsAll = from doc in this._data.Values
                                               from ele in doc.Root.Elements(DefTypeNameOf.JobDef)
                                               where ele.HasField_defName()
                                               select ele;
            int countJobDefs = jobDefsAll.Count();
            if (countJobDefs > 0) {
                Log.Info();
                Log.WriteLine("Start processing JobDefs.");
                foreach (XElement jobDef in jobDefsAll) {
                    XElement reportString = jobDef.Field(FieldNameOf.reportString);
                    if (reportString == null) {
                        jobDef.Add(new XElement(FieldNameOf.reportString, "Doing something."));
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing JobDefs: {0} node(s).", countJobDefs);
            }
        }

        private void CompletePawnKindLabel(DefinitionData definitionDataCore = null) {
            IEnumerable<XElement> pawnKindDefsAll = from doc in this._data.Values
                                                    from ele in doc.Root.Elements(DefTypeNameOf.PawnKindDef)
                                                    where ele.HasField_defName()
                                                    select ele;
            int countPawnKindDefs = pawnKindDefsAll.Count();
            if (countPawnKindDefs > 0) {
                Log.Info();
                Log.WriteLine("Start processing PawnKindDefs.");
                IEnumerable<XElement> racesAll = this.GetAllRaces();
                if (definitionDataCore != null) {
                    racesAll.Concat(definitionDataCore.GetAllRaces());
                }

                foreach (XElement pawnKindDef in pawnKindDefsAll) {
                    bool hasGenders = true;
                    bool isHumanlike = false;
                    XElement raceDefName = pawnKindDef.Field(FieldNameOf.race);
                    if (raceDefName == null) {
                        Log.Error();
                        Log.WriteLine($"The PawnKindDef '{pawnKindDef.Field("defName").Value}' missing the important field 'race'.");
                        continue;
                    }
                    string raceName = raceDefName.Value;
                    foreach (XElement race in racesAll) {
                        if (race.defName().Value == raceName) {
                            XElement field_race = race.Field(FieldNameOf.race);
                            if (field_race != null) {
                                XElement field_hasGenders = field_race.Field(FieldNameOf.hasGenders);
                                if (field_hasGenders != null && !bool.TryParse(field_hasGenders.Value, out hasGenders)) {
                                    hasGenders = true;
                                }
                                XElement field_intelligence = field_race.Field(FieldNameOf.intelligence);
                                if (field_intelligence != null && field_intelligence.Value == "Humanlike") {
                                    isHumanlike = true;
                                }
                            }
                            break;
                        }
                    }
                    if (raceName == "Human") {
                        isHumanlike = true;
                    }


                    // Process root
                    XElement defName = pawnKindDef.defName();

                    // label
                    XElement label = pawnKindDef.label();
                    bool flag_Plural = false;
                    XElement labelPlural = pawnKindDef.Field(FieldNameOf.labelPlural);
                    if (labelPlural != null)
                        flag_Plural = true;

                    // Male
                    bool flag_Male = false;
                    XElement labelMale = pawnKindDef.Field(FieldNameOf.labelMale);
                    if (labelMale != null)
                        flag_Male = true;
                    // MalePlural
                    bool flag_MalePlural = false;
                    XElement labelMalePlural = pawnKindDef.Field(FieldNameOf.labelMalePlural);
                    if (labelMalePlural != null)
                        flag_MalePlural = true;

                    // Female
                    bool flag_Female = false;
                    XElement labelFemale = pawnKindDef.Field(FieldNameOf.labelFemale);
                    if (labelFemale != null)
                        flag_Female = true;
                    // FemalePlural
                    bool flag_FemalePlural = false;
                    XElement labelFemalePlural = pawnKindDef.Field(FieldNameOf.labelFemalePlural);
                    if (labelFemalePlural != null)
                        flag_FemalePlural = true;

                    // label
                    CompleteAndTidy(ref defName, ref label, FieldNameOf.label, defName.Value);
                    // Plural
                    CompleteAndTidy(ref label, ref labelPlural, FieldNameOf.labelPlural, label.Value, "{0}s");

                    // Male
                    CompleteAndTidy(ref labelPlural, ref labelMale, FieldNameOf.labelMale, label.Value, "male {0}");
                    // MalePlural
                    if (flag_Plural) {
                        CompleteAndTidy(ref labelMale, ref labelMalePlural, FieldNameOf.labelMalePlural, labelPlural.Value, "male {0}");
                    } else {
                        CompleteAndTidy(ref labelMale, ref labelMalePlural, FieldNameOf.labelMalePlural, labelMale.Value, "{0}s");
                    }

                    // Female
                    CompleteAndTidy(ref labelMalePlural, ref labelFemale, FieldNameOf.labelFemale, label.Value, "female {0}");
                    // FemalePlural
                    if (flag_Plural) {
                        CompleteAndTidy(ref labelFemale, ref labelFemalePlural, FieldNameOf.labelFemalePlural, labelPlural.Value, "female {0}");
                    } else {
                        CompleteAndTidy(ref labelFemale, ref labelFemalePlural, FieldNameOf.labelFemalePlural, labelFemale.Value, "{0}s");
                    }

                    bool flag_NothingInLifeStage = true;

                    // Process lifeStages
                    XElement lifeStages = pawnKindDef.Field(FieldNameOf.lifeStages);
                    if (lifeStages != null && lifeStages.Elements().Count() > 0) {
                        IEnumerable<XElement> lifeStagesList = lifeStages.Elements();
                        // Patch all labels in lifeStages
                        foreach (XElement curLifeStage in lifeStagesList) {
                            // label
                            bool curflag_label = false;
                            XElement curLabel = curLifeStage.label();
                            if (curLabel != null)
                                curflag_label = true;
                            bool curflag_Plural = false;
                            XElement curLabelPlural = curLifeStage.Field(FieldNameOf.labelPlural);
                            if (curLabelPlural != null)
                                curflag_Plural = true;

                            // labelMale
                            bool curflag_Male = false;
                            XElement curLabelMale = curLifeStage.Field(FieldNameOf.labelMale);
                            if (curLabelMale != null)
                                curflag_Male = true;
                            bool curflag_MalePlural = false;
                            XElement curLabelMalePlural = curLifeStage.Field(FieldNameOf.labelMalePlural);
                            if (curLabelMalePlural != null)
                                curflag_MalePlural = true;

                            // Female
                            bool curflag_Female = false;
                            XElement curLabelFemale = curLifeStage.Field(FieldNameOf.labelFemale);
                            if (curLabelFemale != null)
                                curflag_Female = true;
                            // FemalPlural
                            bool curflag_FemalePlural = false;
                            XElement curLabelFemalePlural = curLifeStage.Field(FieldNameOf.labelFemalePlural);
                            if (curLabelFemalePlural != null)
                                curflag_FemalePlural = true;

                            ////////////////////////////////////////////////////////////////

                            if (lifeStages.Elements().Count() == 1 &&
                                !curflag_label && !curflag_Plural &&
                                !curflag_Male && !curflag_MalePlural &&
                                !curflag_Female && !curflag_FemalePlural) {
                                flag_NothingInLifeStage = true;
                                break;
                            } else {
                                flag_NothingInLifeStage = false;
                            }

                            ////////////////////////////////////////////////////////////////

                            // label
                            if (curLabel == null) {
                                curLifeStage.AddFirst(label);
                                curLabel = curLifeStage.label();
                            } else if (curLabel != curLifeStage.Elements().First()) {
                                curLabel.Remove();
                                curLifeStage.AddFirst(curLabel);
                                curLabel = curLifeStage.Elements().First();
                            }
                            // Plural
                            if (curflag_label) {
                                CompleteAndTidy(ref curLabel, ref curLabelPlural, FieldNameOf.labelPlural, curLabel.Value, "{0}s");
                            } else if (flag_Plural) {
                                CompleteAndTidy(ref curLabel, ref curLabelPlural, FieldNameOf.labelPlural, labelPlural.Value);
                            } else {
                                CompleteAndTidy(ref curLabel, ref curLabelPlural, FieldNameOf.labelPlural, label.Value);
                            }

                            // Male
                            if (curflag_label) {
                                CompleteAndTidy(ref curLabelPlural, ref curLabelMale, FieldNameOf.labelMale, curLabel.Value, "male {0}");
                            } else if (flag_Male) {
                                CompleteAndTidy(ref curLabelPlural, ref curLabelMale, FieldNameOf.labelMale, labelMale.Value);
                            } else {
                                CompleteAndTidy(ref curLabelPlural, ref curLabelMale, FieldNameOf.labelMale, label.Value, "male {0}");
                            }
                            // MalePlural
                            if (curflag_Male) {
                                CompleteAndTidy(ref curLabelMale, ref curLabelMalePlural, FieldNameOf.labelMalePlural, curLabelMale.Value, "{0}s");
                            } else if (curflag_Plural) {
                                CompleteAndTidy(ref curLabelMale, ref curLabelMalePlural, FieldNameOf.labelMalePlural, curLabelPlural.Value, "male {0}");
                            } else if (curflag_label) {
                                CompleteAndTidy(ref curLabelMale, ref curLabelMalePlural, FieldNameOf.labelMalePlural, curLabel.Value, "male {0}s");
                            } else {
                                CompleteAndTidy(ref curLabelMale, ref curLabelMalePlural, FieldNameOf.labelMalePlural, label.Value, "male {0}s");
                            }

                            // Female
                            if (curflag_label) {
                                CompleteAndTidy(ref curLabelMalePlural, ref curLabelFemale, FieldNameOf.labelFemale, curLabel.Value, "female {0}");
                            } else if (flag_Female) {
                                CompleteAndTidy(ref curLabelMalePlural, ref curLabelFemale, FieldNameOf.labelFemale, labelFemale.Value);
                            } else {
                                CompleteAndTidy(ref curLabelMalePlural, ref curLabelFemale, FieldNameOf.labelFemale, label.Value, "female {0}");
                            }
                            // FemalePlural
                            if (curflag_Female) {
                                CompleteAndTidy(ref curLabelFemale, ref curLabelFemalePlural, FieldNameOf.labelFemalePlural, curLabelFemale.Value, "{0}s");
                            } else if (curflag_Plural) {
                                CompleteAndTidy(ref curLabelFemale, ref curLabelFemalePlural, FieldNameOf.labelFemalePlural, curLabelPlural.Value, "female {0}");
                            } else if (curflag_label) {
                                CompleteAndTidy(ref curLabelFemale, ref curLabelFemalePlural, FieldNameOf.labelFemalePlural, curLabel.Value, "female {0}s");
                            } else {
                                CompleteAndTidy(ref curLabelFemale, ref curLabelFemalePlural, FieldNameOf.labelFemalePlural, label.Value, "female {0}s");
                            }

                            // Remove labels according to hasGenders
                            if (!hasGenders) {
                                curLabelMale.Remove();
                                curLabelMalePlural.Remove();
                                curLabelFemale.Remove();
                                curLabelFemalePlural.Remove();
                            }
                        }
                    }

                    // Remove root labels
                    if (!hasGenders) {
                        labelMale.Remove();
                        labelMalePlural.Remove();
                        labelFemale.Remove();
                        labelFemalePlural.Remove();
                    } else if (isHumanlike &&
                          !flag_Male && !flag_MalePlural &&
                          !flag_Female && !flag_FemalePlural) {
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

        /// <summary>
        /// Get all races (ThingDef which is pawn category) in this Defs.
        /// </summary>
        public IEnumerable<XElement> GetAllRaces() {
            foreach (XElement thingDef in from doc in this._data.Values
                                          from ele in doc.Root.Elements(DefTypeNameOf.ThingDef)
                                          where ele.HasField_defName()
                                          select ele) {
                XElement category = thingDef.Field(FieldNameOf.category);
                if (category != null && category.Value == "Pawn") {
                    yield return thingDef;
                }
            }
        }

        private void CompletePawnRelationLabel() {
            IEnumerable<XElement> pawnRelationDefsAll = from doc in this._data.Values
                                                        from ele in doc.Root.Elements(DefTypeNameOf.PawnRelationDef)
                                                        where ele.HasField_defName()
                                                        select ele;
            int countPawnRelationDefs = pawnRelationDefsAll.Count();
            if (countPawnRelationDefs > 0) {
                Log.Info();
                Log.WriteLine("Start processing PawnRelationDefs.");
                foreach (XElement pawnRelationDef in pawnRelationDefsAll) {
                    XElement defName = pawnRelationDef.defName();
                    XElement label = pawnRelationDef.label();
                    CompleteAndTidy(ref defName, ref label, FieldNameOf.label, defName.Value);
                    XElement labelFemale = pawnRelationDef.Field(FieldNameOf.labelFemale);
                    CompleteAndTidy(ref label, ref labelFemale, FieldNameOf.labelFemale, label.Value);
                }
                Log.Info();
                Log.WriteLine("Completed processing PawnRelationDefs: {0} node(s).", countPawnRelationDefs);
            }
        }

        private void CompleteResearchTab() {
            IEnumerable<XElement> researchProjectDefsAll = from doc in this._data.Values
                                                           from ele in doc.Root.Elements(DefTypeNameOf.ResearchProjectDef)
                                                           where ele.HasField_defName()
                                                           select ele;
            int countResearchProjectDefs = researchProjectDefsAll.Count();
            if (countResearchProjectDefs > 0) {
                Log.Info();
                Log.WriteLine("Start processing ResearchProjectDef.");
                foreach (XElement researchProjectDef in researchProjectDefsAll) {
                    XElement tab = researchProjectDef.Field(FieldNameOf.tab);
                    if (tab == null) {
                        researchProjectDef.Add(new XElement(FieldNameOf.tab, "Main"));
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing ResearchProjectDefs: {0} node(s).", countResearchProjectDefs);
            }
        }

        private void CompleteScenarioNameAndDesc() {
            IEnumerable<XElement> scenarioDefsAll = from doc in this._data.Values
                                                    from ele in doc.Root.Elements(DefTypeNameOf.ScenarioDef)
                                                    where ele.HasField_defName()
                                                    select ele;
            int countScenarioDefs = scenarioDefsAll.Count();
            if (countScenarioDefs > 0) {
                Log.Info();
                Log.WriteLine("Start processing ScenarioDefs.");
                foreach (XElement scenarioDef in scenarioDefsAll) {
                    XElement defName = scenarioDef.defName();
                    XElement label = scenarioDef.label();
                    CompleteAndTidy(ref defName, ref label, FieldNameOf.label, defName.Value);
                    XElement description = scenarioDef.description();
                    CompleteAndTidy(ref label, ref description, FieldNameOf.description, label.Value);
                    XElement scenario = scenarioDef.Field(FieldNameOf.scenario);
                    if (scenario != null) {
                        XElement name = scenario.Field(FieldNameOf.name);
                        if (name == null) {
                            scenario.AddFirst(new XElement(FieldNameOf.name, label.Value));
                            name = scenario.Field(FieldNameOf.name);
                        }
                        XElement desc = scenario.description();
                        if (desc == null) {
                            name.AddAfterSelf(new XElement(FieldNameOf.description, description.Value));
                        }
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing ScenarioDefs: {0} node(s).", countScenarioDefs);
            }
        }

        private void CompleteSkillLabel() {
            IEnumerable<XElement> skillDefsAll = from doc in this._data.Values
                                                 from ele in doc.Root.Elements(DefTypeNameOf.SkillDef)
                                                 where ele.HasField_defName()
                                                 select ele;
            int countSkillDefs = skillDefsAll.Count();
            if (countSkillDefs > 0) {
                Log.Info();
                Log.WriteLine("Start processing SkillDefs.");
                foreach (XElement skillDef in skillDefsAll) {
                    XElement defName = skillDef.defName();
                    XElement label = skillDef.label();
                    XElement skillLabel = skillDef.Field(FieldNameOf.skillLabel);
                    if (label == null) {
                        if (skillLabel == null) {
                            defName.AddAfterSelf(new XElement(FieldNameOf.label, defName.Value));
                        } else {
                            defName.AddAfterSelf(new XElement(FieldNameOf.label, skillLabel.Value));
                        }
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing SkillDefs: {0} node(s).", countSkillDefs);
            }
        }

        private void CompleteThingDefStuffAdjective() {
            IEnumerable<XElement> stuffsAll = this.GetAllStuffs();
            int countStuffs = stuffsAll.Count();
            if (countStuffs > 0) {
                Log.Info();
                Log.WriteLine("Start processing StuffAdjective.");
                foreach (XElement stuff in stuffsAll) {
                    XElement defName = stuff.defName();
                    XElement label = stuff.label();
                    CompleteAndTidy(ref defName, ref label, FieldNameOf.label, defName.Value);
                    XElement stuffProps = stuff.Field(FieldNameOf.stuffProps);
                    XElement stuffAdjective = stuffProps.Field(FieldNameOf.stuffAdjective);
                    if (stuffAdjective == null) {
                        stuffProps.AddFirst(new XElement(FieldNameOf.stuffAdjective, label.Value));
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing StuffAdjective: {0} node(s).", countStuffs);
            }
        }

        /// <summary>
        /// Get all stuffs (ThingDef which has stuffProps) in this Defs.
        /// </summary>
        public IEnumerable<XElement> GetAllStuffs() {
            foreach (XElement thingDef in from doc in this._data.Values
                                          from ele in doc.Root.Elements(DefTypeNameOf.ThingDef)
                                          where ele.HasField_defName()
                                          select ele) {
                XElement category = thingDef.Field(FieldNameOf.category);
                XElement stuffProps = thingDef.Field(FieldNameOf.stuffProps);
                if (category != null && category.Value == "Item" && stuffProps != null) {
                    yield return thingDef;
                }
            }
        }

        private void CompleteWorkTypeLabel() {
            IEnumerable<XElement> workTypeDefsAll = from doc in this._data.Values
                                                    from ele in doc.Root.Elements(DefTypeNameOf.WorkTypeDef)
                                                    where ele.HasField_defName()
                                                    select ele;
            int countWorkTypeDefs = workTypeDefsAll.Count();
            if (countWorkTypeDefs > 0) {
                Log.Info();
                Log.WriteLine("Start processing WorkTypeDefs.");
                foreach (XElement workTypeDef in workTypeDefsAll) {
                    XElement defName = workTypeDef.defName();
                    XElement label = workTypeDef.label();
                    XElement labelShort = workTypeDef.Field(FieldNameOf.labelShort);
                    if (label == null) {
                        if (labelShort == null) {
                            defName.AddAfterSelf(new XElement(FieldNameOf.label, defName.Value));
                        } else {
                            defName.AddAfterSelf(new XElement(FieldNameOf.label, labelShort.Value));
                        }
                    }
                }
                Log.Info();
                Log.WriteLine("Completed processing WorkTypeDefs: {0} node(s).", countWorkTypeDefs);
            }
        }

        #endregion

        #region Wiki

        /// <summary>
        /// Process this definition data for MediaWiki.
        /// </summary>
        public void Wiki() {
            this.WikiExtraPawn();
            this.WikiRecipesAddMake();
            this.WikiRecipesAddAdminister();
            this.WikiTerrainAdd();
            this.WikiKeyBindingCategoriesAddArchitect();
            this.WikiKeyBindingsAddMainTab();
        }

        private void WikiExtraPawn() {
            #region Raw Leather
            XElement rawLeather = new XElement(DefTypeNameOf.ThingDef);
            {
                rawLeather.Add(new XComment("Implied ThingDef leather, generated by RimTrans."));
                rawLeather.Add(new XElement("defName", "Temp_Leather"));
                rawLeather.Add(new XElement("label", "temp leather"));
                rawLeather.Add(new XElement("description", "temp leather"));
                rawLeather.Add(new XElement("statBases",
                    new XElement("MaxHitPoints", "100"),
                    new XElement("MarketValue", "2.1"),
                    new XElement("Mass", "0.03"),
                    new XElement("Flammability", "1"),
                    new XElement("Beauty", "-20"),
                    new XElement("DeteriorationRate", "2")
                    ));
                rawLeather.Add(new XElement("pathCost", "15"));
                rawLeather.Add(new XElement("altitudeLayer", "Item"));
                rawLeather.Add(new XElement("thingClass", "ThingWithComps"));
                rawLeather.Add(new XElement("category", "Item"));
                rawLeather.Add(new XElement("stackLimit", "75"));
                rawLeather.Add(new XElement("rotatable", "false"));
                rawLeather.Add(new XElement("useHitPoints", "true"));
                rawLeather.Add(new XElement("comps", XElement.Parse("<li Class=\"CompProperties_Forbiddable\" ListIndex=\"0\" />")));
                rawLeather.Add(new XElement("graphicData",
                    new XElement("texPath", "Things/Item/Resource/Cloth"),
                    new XElement("graphicClass", "Graphic_Single"),
                    new XElement("color", "(132,99,60)"),
                    new XElement("colorTwo", "(132,99,60)")
                    ));
                rawLeather.Add(new XElement("selectable", "true"));
                rawLeather.Add(new XElement("drawGUIOverlay", "true"));
                rawLeather.Add(new XElement("resourceReadoutPriority", "Middle"));
                rawLeather.Add(new XElement("alwaysHaulable", "true"));
                rawLeather.Add(new XElement("thingCategories", XElement.Parse("<li ListIndex=\"0\">Leathers</li>")));
                XElement stuffProps = new XElement("stuffProps");
                stuffProps.Add(new XElement("stuffAdjective", "raw leather"));
                stuffProps.Add(new XElement("commonality", "1.0"));
                stuffProps.Add(new XElement("categories", XElement.Parse("<li ListIndex=\"0\">Leathery</li>")));
                stuffProps.Add(new XElement("statFactors",
                    new XElement("MarketValue", "1.3"),
                    new XElement("ArmorRating_Blunt", "1.5"),
                    new XElement("ArmorRating_Sharp", "1.5"),
                    new XElement("ArmorRating_Heat", "1.7"),
                    new XElement("ArmorRating_Electric", "4"),
                    new XElement("Insulation_Cold", "1"),
                    new XElement("Insulation_Heat", "1")
                    ));
                stuffProps.Add(new XElement("color", "(132,99,60)"));
                rawLeather.Add(stuffProps);
            }
            #endregion
            #region Raw Meat
            XElement rawMeat = new XElement(DefTypeNameOf.ThingDef);
            {
                rawMeat.Add(new XComment("Implied ThingDef meat, generated by RimTrans."));
                rawMeat.Add(new XElement("defName", "RawMeat"));
                rawMeat.Add(new XElement("label", "raw meat"));
                rawMeat.Add(new XElement("description", "raw meat"));
                rawMeat.Add(new XElement("statBases",
                    new XElement("MaxHitPoints", "50"),
                    new XElement("MarketValue", "2"),
                    new XElement("Mass", "0.03"),
                    new XElement("Flammability", "0.5"),
                    new XElement("Beauty", "-20"),
                    new XElement("DeteriorationRate", "10")
                    ));
                rawMeat.Add(new XElement("pathCost", "15"));
                rawMeat.Add(new XElement("altitudeLayer", "Item"));
                rawMeat.Add(new XElement("thingClass", "ThingWithComps"));
                rawMeat.Add(new XElement("category", "Item"));
                rawMeat.Add(new XElement("stackLimit", "75"));
                rawMeat.Add(new XElement("rotatable", "false"));
                rawMeat.Add(new XElement("useHitPoints", "true"));
                rawMeat.Add(new XElement("comps",
                    XElement.Parse("<li Class=\"CompProperties_Forbiddable\" ListIndex=\"0\" />"),
                    XElement.Parse("<li Class=\"CompProperties_Rottable\" ListIndex=\"1\"><daysToRotStart>2</daysToRotStart><rotDestroys>true</rotDestroys></li>"),
                    XElement.Parse("<li Class=\"CompProperties_FoodPoisoningChance\" ListIndex=\"2\" />")
                    ));
                rawMeat.Add(new XElement("graphicData",
                    new XElement("texPath", "Things/Item/Resource/MeatFoodRaw/MeatSmall"),
                    new XElement("graphicClass", "Graphic_Single"),
                    new XElement("color", "(141,56,52)")
                    ));
                rawMeat.Add(new XElement("selectable", "true"));
                rawMeat.Add(new XElement("drawGUIOverlay", "true"));
                rawMeat.Add(new XElement("resourceReadoutPriority", "Middle"));
                rawMeat.Add(new XElement("alwaysHaulable", "true"));
                rawMeat.Add(new XElement("thingCategories", XElement.Parse("<li ListIndex=\"0\">MeatRaw</li>")));
                rawMeat.Add(new XElement("socialPropernessMatters", "true"));
                rawMeat.Add(new XElement("ingestible",
                    new XElement("tasteThought", "AteRawFood"),
                    new XElement("ingestEffect", "EatMeat"),
                    new XElement("ingestSound", "RawMeat_Eat"),
                    new XElement("foodType", "Meat"),
                    new XElement("nutrition", "0.05"),
                    new XElement("preferability", "RawBad")
                    ));
            }
            #endregion
            foreach (XElement curDef in from doc in this._data.Values
                                        from ele in doc.Root.Elements()
                                        where ele.Name.ToString() == DefTypeNameOf.ThingDef
                                        select ele) {
                XElement defName = curDef.defName();
                if (defName == null)
                    continue;

                XElement categeory = curDef.Field(FieldNameOf.category);
                if (categeory == null || categeory.Value != ThingCategoryOf.Pawn)
                    continue;

                XElement race = curDef.Field(FieldNameOf.race);
                if (race == null)
                    continue;

                XElement label = curDef.label();
                if (label == null)
                    label = defName;
                string defNameValue = defName.Value;
                string labelValue = label.Value;

                XElement butcherProducts = curDef.Field(FieldNameOf.butcherProducts);
                XElement LeatherAmount = null;
                XElement MeatAmount = null;
                XElement statBases = curDef.Field(FieldNameOf.statBases);
                if (statBases != null) {
                    LeatherAmount = statBases.Element("LeatherAmount");
                    MeatAmount = statBases.Element("MeatAmount");
                }
                XElement fleshType = race.Field(FieldNameOf.fleshType);
                XElement useLeatherFrom = race.Field(FieldNameOf.useLeatherFrom);
                XElement useMeatFrom = race.Field(FieldNameOf.useMeatFrom);

                if (fleshType != null && fleshType.Value == "Mechanoid") {

                } else {
                    // Leather
                    XElement leatherAfterRace = null;
                    if (butcherProducts != null) {

                    } else if (LeatherAmount != null && LeatherAmount.Value == "0") {

                    } else if (useLeatherFrom != null) {

                    } else {
                        #region GenerateThingDef Leather
                        XElement leatherLabel = race.Field(FieldNameOf.leatherLabel);
                        string leatherLabelValue =
                            leatherLabel == null ?
                            string.Format("{0} leather", labelValue) :
                            leatherLabel.Value;
                        XElement leather = new XElement(rawLeather);
                        leather.Element("defName").Value = defNameValue + "_Leather";
                        leather.Element("label").Value = leatherLabelValue;
                        leather.Element("description").Value = leatherLabelValue;
                        leather.Element("stuffProps").Element("stuffAdjective").Value = leatherLabelValue;
                        XElement leatherColor = race.Field(FieldNameOf.leatherColor);
                        if (leatherColor != null) {
                            leather.Element("graphicData").Element("color").Value = leatherColor.Value;
                            leather.Element("graphicData").Element("colorTwo").Value = leatherColor.Value;
                            leather.Element("stuffProps").Element("color").Value = leatherColor.Value;
                        }
                        XElement leatherCommonalityFactor = race.Field(FieldNameOf.leatherCommonalityFactor);
                        if (leatherCommonalityFactor != null) {
                            float factor;
                            if (float.TryParse(leatherCommonalityFactor.Value, out factor)) {
                                leather.Element("stuffProps").Element("commonality").Value = (1.0f * factor).ToString();
                            }
                        }
                        XElement leatherInsulation = race.Field(FieldNameOf.leatherInsulation);
                        if (leatherInsulation != null) {
                            leather.Element("stuffProps").Element("statFactors").Element("Insulation_Cold").Value = leatherInsulation.Value;
                        }
                        XElement leatherStatFactors = race.Field(FieldNameOf.leatherStatFactors);
                        if (leatherStatFactors != null) {
                            XElement statFactors = leather.Element("stuffProps").Element("statFactors");
                            foreach (XElement leatherStat in leatherStatFactors.Elements()) {
                                statFactors.SetElementValue(leatherStat.Name, leatherStat.Value);
                            }
                        }
                        XElement leatherMarketValueFactor = race.Field(FieldNameOf.leatherMarketValueFactor);
                        if (leatherMarketValueFactor != null) {
                            float factor;
                            if (float.TryParse(leatherMarketValueFactor.Value, out factor)) {
                                leather.Element("statBases").Element("MarketValue").Value = (2.1f * factor).ToString();
                            }
                        }
                        curDef.AddAfterSelf(leather);
                        leatherAfterRace = curDef.ElementsAfterSelf().First();
                        #endregion
                    }

                    // Meat
                    if (MeatAmount != null && MeatAmount.Value == "0") {

                    } else if (useMeatFrom != null) {

                    } else {
                        #region GenerateThingDef Meat
                        XElement meatLabel = race.Field(FieldNameOf.meatLabel);
                        string meatLabelValue =
                            meatLabel == null ?
                            string.Format("{0} meat", labelValue) :
                            meatLabel.Value;
                        XElement meat = new XElement(rawMeat);
                        meat.Element("defName").Value = defNameValue + "_Meat";
                        meat.Element("label").Value = meatLabelValue;
                        meat.Element("description").Value = meatLabelValue;
                        if (fleshType != null && fleshType.Value == "Insectoid") {
                            XElement ingestible = meat.Element("ingestible");
                            ingestible.SetElementValue("specialThoughtDirect", "AteInsectMeatDirect");
                            ingestible.SetElementValue("specialThoughtAsIngredient", "AteInsectMeatAsIngredient");
                        }
                        XElement intelligence = race.Field(FieldNameOf.intelligence);
                        if (intelligence != null && intelligence.Value == "Humanlike") {
                            meat.Element("graphicData").Element("texPath").Value = "Things/Item/Resource/MeatFoodRaw/MeatHuman";
                        } else {
                            XElement baseBodySize = race.Field(FieldNameOf.baseBodySize);
                            if (baseBodySize != null) {
                                float size;
                                if (float.TryParse(baseBodySize.Value, out size)) {
                                    if (size >= 0.7f) {
                                        meat.Element("graphicData").Element("texPath").Value = "Things/Item/Resource/MeatFoodRaw/MeatSmall";
                                    } else {
                                        meat.Element("graphicData").Element("texPath").Value = "Things/Item/Resource/MeatFoodRaw/MeatBig";
                                    }
                                }
                            }
                        }
                        XElement meatColor = race.Field(FieldNameOf.meatColor);
                        if (meatColor != null) {
                            meat.Element("graphicData").Element("color").Value = meatColor.Value;
                        }
                        if (leatherAfterRace != null) {
                            leatherAfterRace.AddAfterSelf(meat);
                        } else {
                            curDef.AddAfterSelf(meat);
                        }
                        #endregion
                    }
                }
            }
        }

        private void WikiRecipesAddMake() {
            XDocument docRecipeAddMake = DocHelper.EmptyDocDef();
            docRecipeAddMake.Root.Add(new XComment("Implied RecipeDef from recipeMaker, generated by RimTrans."));
            foreach (XElement curDef in from doc in this._data.Values
                                        from ele in doc.Root.Elements()
                                        where ele.Name.ToString() == DefTypeNameOf.ThingDef
                                        select ele) {
                XElement defName = curDef.defName();
                if (defName == null)
                    continue;

                XElement recipeMaker = curDef.Field(FieldNameOf.recipeMaker);
                if (recipeMaker == null)
                    continue;

                #region GenerateRecipeDef Make
                XElement label = curDef.label();
                if (label == null)
                    label = defName;
                string labelValue = label.Value;
                XElement recipe = new XElement(DefTypeNameOf.RecipeDef);
                recipe.Add(new XElement("defName", "Make_" + defName.Value));
                recipe.Add(new XElement("label", "make " + labelValue));
                recipe.Add(new XElement("description", $"Make {labelValue}."));
                recipe.Add(new XElement("jobString", $"Making {labelValue}."));
                XElement statBases = curDef.Field(FieldNameOf.statBases);
                if (statBases != null) {
                    XElement WorkToMake = statBases.Element("WorkToMake");
                    if (WorkToMake != null)
                        recipe.Add(new XElement("workAmount", WorkToMake.Value));
                }
                XElement workSpeedStat = recipeMaker.Field(FieldNameOf.workSpeedStat);
                if (workSpeedStat != null)
                    recipe.Add(workSpeedStat);
                XElement efficiencyStat = recipeMaker.Field(FieldNameOf.efficiencyStat);
                if (efficiencyStat != null)
                    recipe.Add(efficiencyStat);
                XElement ingredients = new XElement("ingredients");
                XElement fixedIngredientFilter = new XElement("fixedIngredientFilter");
                int countIngredients = 0;
                bool productHasIngredientStuff = false;
                {
                    XElement stuffCategories = curDef.Field(FieldNameOf.stuffCategories);
                    XElement costStuffCount = curDef.Field(FieldNameOf.costStuffCount);
                    if (costStuffCount != null && stuffCategories != null && stuffCategories.HasElements) {
                        productHasIngredientStuff = true;
                        XElement ingredientCount = XElement.Parse($"<li ListIndex=\"{countIngredients}\" />");
                        ingredientCount.Add(
                            new XElement("filter", new XElement("categories", stuffCategories.Elements())),
                            new XElement("count", costStuffCount.Value)
                            );
                        ingredients.Add(ingredientCount);
                        countIngredients++;
                        fixedIngredientFilter.Add(new XElement("categories", stuffCategories.Elements()));
                    }
                }
                {
                    XElement costList = curDef.Field(FieldNameOf.costList);
                    if (costList != null && costList.HasElements) {
                        XElement thingDefs = new XElement("thingDefs");
                        int countThingDefs = 0;
                        foreach (XElement thingCount in costList.Elements()) {
                            XElement ingredientCount = XElement.Parse($"<li ListIndex=\"{countIngredients}\" />");
                            ingredientCount.Add(
                                new XElement("filter", new XElement("thingDefs", XElement.Parse($"<li ListIndex=\"0\">{thingCount.Name.ToString()}</li>"))),
                                new XElement("count", thingCount.Value)
                                );
                            ingredients.Add(ingredientCount);
                            countIngredients++;
                            thingDefs.Add(XElement.Parse($"<li ListIndex=\"{countThingDefs}\">{thingCount.Name.ToString()}</li>"));
                            countThingDefs++;
                        }
                        fixedIngredientFilter.AddFirst(thingDefs);
                    }
                }
                recipe.Add(ingredients);
                recipe.Add(fixedIngredientFilter);
                XElement defaultIngredientFilter = recipeMaker.Field(FieldNameOf.defaultIngredientFilter);
                if (defaultIngredientFilter != null)
                    recipe.Add(defaultIngredientFilter);
                if (productHasIngredientStuff)
                    recipe.Add(new XElement("productHasIngredientStuff", "true"));
                recipe.Add(new XElement("products",
                    new XElement(defName.Value, "1")
                    ));
                XElement unfinishedThingDef = recipeMaker.Field(FieldNameOf.unfinishedThingDef);
                if (unfinishedThingDef != null)
                    recipe.Add(unfinishedThingDef);
                XElement skillRequirements = recipeMaker.Field(FieldNameOf.skillRequirements);
                if (skillRequirements != null)
                    recipe.Add(skillRequirements);
                XElement workSkill = recipeMaker.Field(FieldNameOf.workSkill);
                if (workSkill != null)
                    recipe.Add(workSkill);
                XElement workSkillLearnPerTick = recipeMaker.Field(FieldNameOf.workSkillLearnPerTick);
                if (workSkillLearnPerTick != null)
                    recipe.Add(new XElement("workSkillLearnFactor", workSkillLearnPerTick.Value));
                XElement effectWorking = recipeMaker.Field(FieldNameOf.effectWorking);
                if (effectWorking != null)
                    recipe.Add(effectWorking);
                XElement soundWorking = recipeMaker.Field(FieldNameOf.soundWorking);
                if (soundWorking != null)
                    recipe.Add(soundWorking);
                XElement recipeUsers = recipeMaker.Field(FieldNameOf.recipeUsers);
                if (recipeUsers != null)
                    recipe.Add(recipeUsers);
                XElement researchPrerequisite = recipeMaker.Field(FieldNameOf.researchPrerequisite);
                if (researchPrerequisite != null)
                    recipe.Add(researchPrerequisite);
                #endregion
                docRecipeAddMake.Root.Add(recipe);
            }
            this._data.Add(@"RecipeDefs\Recipes_Add_Make.xml", docRecipeAddMake);
        }

        private void WikiRecipesAddAdminister() {
            #region recipeUsers
            XElement recipeUsers = new XElement("recipeUsers");
            {
                int countUsers = 0;
                foreach (XElement curThingDef in from doc in this._data.Values
                                                 from ele in doc.Root.Elements()
                                                 where ele.Name.ToString() == DefTypeNameOf.ThingDef
                                                 select ele) {
                    XElement category = curThingDef.Field(FieldNameOf.category);
                    if (category == null || category.Value != ThingCategoryOf.Pawn)
                        continue;

                    XElement defName = curThingDef.defName();
                    if (defName == null)
                        continue;

                    XElement race = curThingDef.Field(FieldNameOf.race);
                    if (race == null)
                        continue;

                    XElement fleshType = race.Field(FieldNameOf.fleshType);
                    if (fleshType != null && fleshType.Value == "Mechanoid")
                        continue;

                    recipeUsers.Add(XElement.Parse($"<li ListIndex=\"{countUsers}\">={defName.Value}</li>"));
                }
            }
            #endregion
            XDocument docRecipesAddAdminister = DocHelper.EmptyDocDef();
            docRecipesAddAdminister.Add(new XComment("Implied RecipeDef from drugs, generated by RimTrans."));
            foreach (XElement curDef in from doc in this._data.Values
                                        from ele in doc.Root.Elements()
                                        where ele.Name.ToString() == DefTypeNameOf.ThingDef
                                        select ele) {
                XElement category = curDef.Field(FieldNameOf.category);
                if (category == null || category.Value != ThingCategoryOf.Item)
                    continue;

                XElement defName = curDef.defName();
                if (defName == null)
                    continue;

                XElement ingestible = curDef.Field(FieldNameOf.ingestible);
                if (ingestible == null)
                    continue;

                XElement drugCategory = ingestible.Field(FieldNameOf.drugCategory);
                if (drugCategory == null || drugCategory.Value == "None")
                    continue;

                #region GenerateRecipeDef Administer
                string defNameValue = defName.Value;
                XElement label = curDef.label();
                if (label == null)
                    label = defName;
                string labelValue = label.Value;
                XElement administer = new XElement(DefTypeNameOf.RecipeDef);
                administer.Add(new XElement("defName", "Administer_" + defName.Value));
                administer.Add(new XElement("label", "administer " + labelValue));
                administer.Add(new XElement("jobString", $"Administering {labelValue}."));
                administer.Add(new XElement("workerClass", "Recipe_AdministerIngestible"));
                XElement baseIngestTicks = ingestible.Field(FieldNameOf.baseIngestTicks);
                if (baseIngestTicks != null)
                    administer.Add(new XElement("workAmount", baseIngestTicks.Value));
                administer.Add(new XElement("ingredients",
                    XElement.Parse($"<li ListIndex=\"0\"><filter><thingDefs><li ListIndex=\"0\">{defNameValue}</li></thingDefs></filter><count>1</count></li>")
                    ));
                administer.Add(new XElement("fixedIngredientFilter",
                    new XElement("thingDefs", XElement.Parse($"<li ListIndex=\"0\">{defNameValue}</li>"))
                    ));
                administer.Add(new XElement("targetsBodyPart", "false"));
                administer.Add(new XElement("anesthetize", "false"));
                #endregion
                docRecipesAddAdminister.Root.Add(administer);
            }
            this._data.Add(@"RecipeDefs\Recipes_Add_Administer.xml", docRecipesAddAdminister);
        }

        private void WikiTerrainAdd() {
            #region Raw Terrain
            XElement rawTerrain = new XElement("TerrainDef");
            {
                rawTerrain.Add(new XElement("defName", "RawTerrain"));
                rawTerrain.Add(new XElement("label", "raw terrain"));
                rawTerrain.Add(new XElement("description", "raw terrain"));
                rawTerrain.Add(new XElement("statBases",
                    new XElement("Beauty", "-1")
                    ));
                rawTerrain.Add(new XElement("pathCost", "1"));
                rawTerrain.Add(new XElement("fertility", "0"));
                rawTerrain.Add(new XElement("texturePath", "Terrain/Surfaces/RoughStone"));
                rawTerrain.Add(new XElement("edgeType", "FadeRough"));
                rawTerrain.Add(new XElement("renderPrecedence", "190"));
                rawTerrain.Add(new XElement("affordances"));
                rawTerrain.Add(new XElement("scatterType", "Rocky"));
                rawTerrain.Add(new XElement("smoothedTerrain"));
                rawTerrain.Add(new XElement("color", "(255,255,255)"));
                rawTerrain.Add(new XElement("acceptTerrainSourceFilth", "false"));
                rawTerrain.Add(new XElement("acceptFilth", "true"));
            }
            #endregion
            XDocument docTerrainAdd = DocHelper.EmptyDocDef();
            docTerrainAdd.Root.Add(new XComment("Implied TerraindDef, generated by RimTrans."));
            foreach (XElement curDef in from doc in this._data.Values
                                        from ele in doc.Root.Elements()
                                        select ele) {
                if (curDef.Name.ToString() != DefTypeNameOf.ThingDef)
                    continue;

                XElement defName = curDef.defName();
                if (defName == null)
                    continue;

                XElement category = curDef.Field(FieldNameOf.category);
                if (category == null || category.Value != ThingCategoryOf.Building)
                    continue;

                XElement building = curDef.Field(FieldNameOf.building);
                if (building == null)
                    continue;

                XElement isNaturalRock = building.Field(FieldNameOf.isNaturalRock);
                XElement isResourceRock = building.Field(FieldNameOf.isResourceRock);
                if (isNaturalRock != null && string.Compare(isNaturalRock.Value, "true", true) == 0 &&
                    (isResourceRock == null || string.Compare(isResourceRock.Value, "false", true) == 0)) {
                    string defNameValue = defName.Value;
                    XElement label = curDef.label();
                    if (label == null)
                        label = defName;
                    string labelValue = label.Value;
                    #region GenerateTerrainDef
                    XElement graphicData = curDef.Field(FieldNameOf.graphicData);
                    XElement color = graphicData == null ? null : graphicData.Field(FieldNameOf.color);
                    XElement rough = new XElement(rawTerrain);
                    {
                        rough.Element("defName").Value = defNameValue + "_Rough";
                        rough.Element("label").Value = "rough " + labelValue;
                        rough.Element("description").Value = $"Rough, natural {labelValue} ground.";
                        rough.Element("texturePath").Value = "Terrain/Surfaces/RoughStone";
                        rough.Element("renderPrecedence").Value = "190";
                        rough.Element("affordances").Add(
                            XElement.Parse("<li ListIndex=\"0\">Light</li>"),
                            XElement.Parse("<li ListIndex=\"1\">Heavy</li>"),
                            XElement.Parse("<li ListIndex=\"2\">SmoothableStone</li>")
                            );
                        rough.Element("smoothedTerrain").Value = defNameValue + "_Smooth";
                        if (color != null)
                            rough.Element("color").Value = color.Value;
                    }
                    XElement hewn = new XElement(rawTerrain);
                    {
                        hewn.Element("defName").Value = defNameValue + "_RoughHewn";
                        hewn.Element("label").Value = "rough-hewn " + labelValue;
                        hewn.Element("description").Value = $"Roughly cut natural {labelValue} floor.";
                        hewn.Element("texturePath").Value = "Terrain/Surfaces/RoughHewnRock";
                        hewn.Element("renderPrecedence").Value = "50";
                        hewn.Element("affordances").Add(
                            XElement.Parse("<li ListIndex=\"0\">Light</li>"),
                            XElement.Parse("<li ListIndex=\"1\">Heavy</li>"),
                            XElement.Parse("<li ListIndex=\"2\">SmoothableStone</li>")
                            );
                        hewn.Element("smoothedTerrain").Value = defNameValue + "_Smooth";
                        if (color != null)
                            hewn.Element("color").Value = color.Value;
                    }
                    XElement smooth = new XElement(rawTerrain);
                    {
                        smooth.Element("defName").Value = defNameValue + "_Smooth";
                        smooth.Element("label").Value = "smooth " + labelValue;
                        smooth.Element("description").Value = $"Smoothed natural {labelValue} floor.";
                        smooth.Element("statBases").Element("Beauty").Value = "3";
                        smooth.Element("texturePath").Value = "Terrain/Surfaces/SmoothStone";
                        smooth.Element("renderPrecedence").Value = "140";
                        smooth.Element("affordances").Add(
                            XElement.Parse("<li ListIndex=\"0\">Light</li>"),
                            XElement.Parse("<li ListIndex=\"1\">Heavy</li>"),
                            XElement.Parse("<li ListIndex=\"2\">SmoothHard</li>")
                            );
                        if (color != null)
                            smooth.Element("color").Value = color.Value;
                        smooth.Element("acceptTerrainSourceFilth").Value = "true";
                    }
                    docTerrainAdd.Root.Add(rough, hewn, smooth);
                    #endregion
                }
            }
            this._data.Add(@"TerrainDefs\Terrain_Add.xml", docTerrainAdd);
        }

        private void WikiKeyBindingCategoriesAddArchitect() {
            #region gameUniversalCats
            XElement gameUniversalCats = new XElement("checkForConflicts");
            int countUniversal = 0;
            foreach (XElement KeyBindingCategoryDef in from doc in this._data.Values
                                                       from ele in doc.Root.Elements()
                                                       where ele.Name.ToString() == DefTypeNameOf.KeyBindingCategoryDef && ele.HasField_defName()
                                                       select ele) {
                gameUniversalCats.Add(XElement.Parse($"<li ListIndex=\"{countUniversal}\">{KeyBindingCategoryDef.defName().Value}</li>"));
                countUniversal++;
            }
            #endregion
            XDocument docKeyBindingCategoriesAddArchitect = DocHelper.EmptyDocDef();
            docKeyBindingCategoriesAddArchitect.Root.Add(new XComment("Implied KeyBindingCategoryDef, generated by RimTrans."));
            foreach (XElement curDef in from doc in this._data.Values
                                        from ele in doc.Root.Elements()
                                        select ele) {
                if (curDef.Name.ToString() != DefTypeNameOf.DesignationCategoryDef)
                    continue;

                XElement defName = curDef.defName();
                if (defName == null)
                    continue;

                #region GenerateKeyBindingCategoryDef
                XElement label = curDef.label();
                if (label == null)
                    label = defName;
                string labelValue = label.Value;
                XElement catDef = new XElement(DefTypeNameOf.KeyBindingCategoryDef);
                catDef.Add(new XElement("defName", "Architect_" + defName.Value));
                catDef.Add(new XElement("label", labelValue + " tab"));
                catDef.Add(new XElement("description", $"Key bindings for the \"{labelValue}\" section of the Architect menu"));
                catDef.Add(gameUniversalCats);
                docKeyBindingCategoriesAddArchitect.Root.Add(catDef);
                #endregion
            }
            this._data.Add(@"Misc\KeyBindings\KeyBindingCategories_Add_Architect.xml", docKeyBindingCategoriesAddArchitect);
        }

        private void WikiKeyBindingsAddMainTab() {
            XDocument docKeyBindingsAdd = DocHelper.EmptyDocDef();
            docKeyBindingsAdd.Root.Add(new XComment("Implied KeyBingdingDef, generated by RimTrans."));
            foreach (XElement curDef in from doc in this._data.Values
                                        from ele in doc.Root.Elements()
                                        select ele) {
                if (curDef.Name.ToString() != DefTypeNameOf.MainButtonDef)
                    continue;

                XElement defName = curDef.defName();
                if (defName == null)
                    continue;

                XElement defaultHotKey = curDef.Field(FieldNameOf.defaultHotKey);
                if (defaultHotKey == null)
                    continue;

                #region GenerateKeyBindingDef
                XElement label = curDef.label();
                if (label == null)
                    label = defName;
                string labelValue = label.Value;
                XElement keyDef = new XElement(DefTypeNameOf.KeyBindingDef);
                keyDef.Add(new XElement("defName", "MainTab_" + defName.Value));
                keyDef.Add(new XElement("label", $"Toggle {labelValue} tab"));
                keyDef.Add(new XElement("category", "MainTabs"));
                keyDef.Add(new XElement("defaultKeyCodeA", defaultHotKey.Value));
                #endregion
                docKeyBindingsAdd.Root.Add(keyDef);
            }
            this._data.Add(@"Misc\KeyBindings\KeyBindings_Add_MainTab.xml", docKeyBindingsAdd);
        }

        #endregion

        #region Mark Index

        /// <summary>
        /// Mark index number for list items.
        /// </summary>
        private void MarkIndex() {
            IEnumerable<XElement> defsAll = from doc in this._data.Values
                                            from ele in doc.Root.Elements()
                                            where ele.HasField_defName()
                                            select ele;
            int countDefs = defsAll.Count();
            if (countDefs > 0) {
                Log.Info();
                Log.WriteLine("Start marking index number for list items.");
                foreach (XElement def in defsAll) {
                    MarkIndexRecursively(def); // Recursively
                }
                Log.Info();
                Log.WriteLine("Completed marking index number for list items.");
            }
        }

        /// <summary>
        /// Mark Index Recursively
        /// </summary>
        private static void MarkIndexRecursively(XElement ele) {
            if (ele.HasElements) {
                IEnumerable<XElement> children = ele.Elements();
                IEnumerable<XElement> listItems = ele.Elements("li");
                if (children.Count() == listItems.Count()) {
                    int index = 0;
                    foreach (XElement child in children) {
                        child.SetAttributeValue("ListIndex", index);
                        index++;
                        MarkIndexRecursively(child);
                    }
                } else {
                    foreach (XElement child in ele.Elements()) {
                        MarkIndexRecursively(child);
                    }
                }
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Save this definition data to a directory. All the existed files will be deleted.
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path) {
            if (this._data.Count() == 0)
                return;

            if (Directory.Exists(path)) {
                DirectorySecurity ds = new DirectorySecurity(path, AccessControlSections.Access);
                if (ds.AreAccessRulesProtected) {
                    Log.Error();
                    Log.WriteLine("Outputing Defs failed: No write permission to directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    return;
                } else {
                    DirectoryHelper.CleanDirectory(path, "*.xml");
                }
            } else {
                try {
                    Directory.CreateDirectory(path);
                } catch (Exception ex) {
                    Log.Error();
                    Log.WriteLine("Outputing Defs failed: Can not create directory: ");
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Red, path);
                    Log.Indent();
                    Log.WriteLine(ex.Message);
                    return;
                }
            }

            Log.Info();
            Log.Write("Start outputing Defs: ");
            Log.WriteLine(ConsoleColor.Cyan, path);

            int countValidFiles = 0;
            int countInvalidFiles = 0;
            foreach (KeyValuePair<string, XDocument> relativePathDoc in this._data) {
                string filePath = Path.Combine(path, relativePathDoc.Key);
                string subDirPath = Path.GetDirectoryName(filePath);
                if (Directory.Exists(subDirPath)) {
                    DirectorySecurity curDs = new DirectorySecurity(subDirPath, AccessControlSections.Access);
                    if (curDs.AreAccessRulesProtected) {
                        Log.Error();
                        Log.WriteLine("Outputing to sub-directory failed: No write permission to directory.");
                        Log.Indent();
                        Log.WriteLine(ConsoleColor.Red, subDirPath);
                        continue;
                    }
                } else {
                    Directory.CreateDirectory(subDirPath);
                }
                XDocument doc = relativePathDoc.Value;
                XElement root = doc.Root;
                bool isSpecial = false;
                foreach (XElement def in root.Elements()) {
                    string defTypeName = def.Name.ToString();
                    if (defTypeName == DefTypeNameOf.InteractionDef ||
                        defTypeName == DefTypeNameOf.RulePackDef ||
                        defTypeName == DefTypeNameOf.TaleDef)
                        isSpecial = true;
                }
                if (isSpecial) {
                    // Special for these 3 DefType
                    string text = root.ToString().Replace("-&gt;", "->");
                    try {
                        using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
                            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) // UTF-8 with BOM
                            {
                                sw.WriteLine(doc.Declaration.ToString());
                                sw.Write(text);
                            }
                        }
                        countValidFiles++;
                    } catch (Exception ex) {
                        Log.Error();
                        Log.Write("Outputing file failed: ");
                        Log.WriteLine(ConsoleColor.Red, filePath);
                        Log.Indent();
                        Log.WriteLine(ex.Message);
                        countInvalidFiles++;
                    }
                } else {
                    // Universal
                    try {
                        relativePathDoc.Value.Save(filePath);
                        countValidFiles++;
                    } catch (Exception ex) {
                        Log.Error();
                        Log.Write("Outputing file failed: ");
                        Log.WriteLine(ConsoleColor.Red, filePath);
                        Log.Indent();
                        Log.WriteLine(ex.Message);
                        countInvalidFiles++;
                    }
                }
            }
            if (countValidFiles > 0) {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine($"Completed outputing Defs: {countValidFiles} file(s).");
                } else {
                    Log.Warning();
                    Log.WriteLine($"Completed outputing Defs: Success: {countValidFiles} file(s), Failure {countInvalidFiles} file(s).");
                }
            } else {
                if (countInvalidFiles == 0) {
                    Log.Info();
                    Log.WriteLine("No Defs to be output.");
                } else {
                    Log.Error();
                    Log.WriteLine($"Outputing Defs failed: {countInvalidFiles} file(s).");
                }
            }
        }

        #endregion

        #region Debug

        public void Debug(string fileName) {
            XDocument doc;
            if (this._data.TryGetValue(fileName, out doc)) {
                Log.Write(ConsoleColor.Cyan, fileName);
                Log.WriteLine(doc.ToString());
            }
        }

        public void Debug() {
            Log.WriteLine(ConsoleColor.Cyan, "DefinitionData.Debug()");
            foreach (var fileNameDocPair in this._data) {
                Log.WriteLine(fileNameDocPair.Key);
            }
            //Log.WriteLine("================");
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
            Debug(@"D:\Game\RimWorld\Mods\Core\Defs\ThingDefs_Buildings\Buildings_Production.xml");
        }

        #endregion
    }
}
