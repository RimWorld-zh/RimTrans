using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RimTrans.Builder.Xml {
    public static class XmlHelper {
        #region Def and Field extensions

        public static XElement Field(this XElement def, string fieldName) {
            foreach (XElement field in def.Elements()) {
                if (string.Compare(field.Name.ToString(), fieldName, true) == 0) {
                    return field;
                }
            }
            return null;
        }

        public static bool HasField_defName(this XElement def) {
            foreach (XElement field in def.Elements()) {
                if (string.Compare(field.Name.ToString(), "defName", true) == 0) {
                    return true;
                }
            }
            return false;
        }

        public static XElement defName(this XElement def) {
            foreach (XElement field in def.Elements()) {
                if (string.Compare(field.Name.ToString(), "defName", true) == 0) {
                    return field;
                }
            }
            return null;
        }

        public static XElement label(this XElement def) {
            foreach (XElement field in def.Elements()) {
                if (string.Compare(field.Name.ToString(), "label", true) == 0) {
                    return field;
                }
            }
            return null;
        }

        public static XElement description(this XElement def) {
            foreach (XElement field in def.Elements()) {
                if (string.Compare(field.Name.ToString(), "description", true) == 0) {
                    return field;
                }
            }
            return null;
        }

        public static bool Match(this XElement fieldThis, XElement fieldOther) {
            string nameThis = fieldThis.Name.ToString();
            string nameOther = fieldOther.Name.ToString();
            if (string.Compare(nameThis, nameOther, true) == 0) {
                int defNameLength = nameThis.IndexOf('.');
                if (defNameLength > 0 &&
                    nameThis.Substring(0, defNameLength) == nameOther.Substring(0, defNameLength)) {
                    return true;
                }
            }
            return false;
        }

        public static string BaseInfo(this XElement def) {
            StringBuilder sb = new StringBuilder($" <{def.Name}");
            if (def.HasAttributes) {
                def.Attributes().ToList().ForEach(attr => {
                    sb.Append($" {attr.Name}=\"{attr.Value}\"");
                });
            }
            sb.Append(">");
            if (def.HasField_defName()) {
                sb.Append(def.defName());
            }
            return sb.ToString();
        }

        public static XElement BelongedDef(this XElement field) {
            XElement parent = field.Parent;
            while (parent.Parent?.Parent != null) {
                parent = parent.Parent;
            }
            return parent;
        }

        #endregion

        #region Judge Injectable Def

        public static bool IsNeedToTranslate(this XElement def) {
            string defTypeName = def.Name.ToString();
            if (defTypeName == "Def" ||
                defTypeName == "BuildableDef" ||
                defTypeName == "DesignationDef" ||
                defTypeName == "DutyDef" ||
                defTypeName == "EffecterDef" ||
                defTypeName == "GenStepDef" ||
                defTypeName == "JoyGiverDef" ||
                defTypeName == "MapGeneratorDef" ||
                defTypeName == "PawnGroupKindDef" ||
                defTypeName == "RuleDef" ||
                defTypeName == "ScatterableDef" ||
                defTypeName == "SongDef" ||
                defTypeName == "SoundDef" ||
                defTypeName == "ThinkTreeDef") {
                return false;
            }
            return true;
        }

        #endregion

        #region Judge Injectable Field

        public static bool IsInjectable(this XElement field) {
            string fieldName = field.Name.ToString();
            if (fieldName == "li") {
                string fieldNameParent = field.Parent.Name.ToString();
                if (string.Compare(fieldNameParent, "rulesStrings", true) == 0 ||
                    string.Compare(fieldNameParent, "shuffledNames", true) == 0) // For RimTrans.Framework
                {
                    return true;
                }
            } else if (
                  // Def
                  string.Compare(fieldName, "label", true) == 0 ||
                  string.Compare(fieldName, "description", true) == 0 ||

                  // ConceptDef
                  string.Compare(fieldName, "helpText", true) == 0 ||

                  // FactionDef
                  string.Compare(fieldName, "pawnsPlural", true) == 0 ||
                  string.Compare(fieldName, "fixedName", true) == 0 ||
                  string.Compare(fieldName, "leaderTitle", true) == 0 ||

                  // HediffDef
                  string.Compare(fieldName, "labelNoun", true) == 0 ||

                  // HediffGiverSetDef
                  string.Compare(fieldName, "letterLabel", true) == 0 ||
                  string.Compare(fieldName, "letter", true) == 0 ||

                  // HistoryAutoRecorderDef
                  string.Compare(fieldName, "graphLabelY", true) == 0 ||

                  // IncidentDef
                  //string.Compare(fieldName, "letterLabel", true) == 0 ||
                  string.Compare(fieldName, "letterText", true) == 0 ||

                  // InstructionDef
                  string.Compare(fieldName, "text", true) == 0 ||
                  string.Compare(fieldName, "rejectInputMessage", true) == 0 ||
                  string.Compare(fieldName, "onMapInstruction", true) == 0 ||

                  // LifeStageDef
                  string.Compare(fieldName, "adjective", true) == 0 ||

                  // PawnColumnDef
                  string.Compare(fieldName, "headerTip", true) == 0 ||

                  // PawnRelationDef
                  //string.Compare(fieldName, "labelFemale", true) == 0 ||

                  // RaidStrategyDef
                  string.Compare(fieldName, "letterLabelEnemy", true) == 0 ||
                  string.Compare(fieldName, "arrivalTextEnemy", true) == 0 ||
                  string.Compare(fieldName, "letterLabelFriendly", true) == 0 ||
                  string.Compare(fieldName, "arrivalTextFriendly", true) == 0 ||

                  // ScenarioDef
                  string.Compare(fieldName, "name", true) == 0 ||
                  string.Compare(fieldName, "summary", true) == 0 ||
                  //string.Compare(fieldName, "text", true) == 0 ||

                  // SitePartDef
                  string.Compare(fieldName, "descriptionDialogue", true) == 0 ||

                  // SkillDef
                  string.Compare(fieldName, "skillLabel", true) == 0 ||
                  string.Compare(fieldName, "pawnLabel", true) == 0 ||

                  // StatDef
                  string.Compare(fieldName, "customLabel", true) == 0 ||
                  string.Compare(fieldName, "formatString", true) == 0 ||

                  // ThoughtDef
                  string.Compare(fieldName, "labelSocial", true) == 0 ||

                  // WorkGiverDef
                  string.Compare(fieldName, "verb", true) == 0 ||
                  string.Compare(fieldName, "gerund", true) == 0 ||

                  // Verse

                  // DamageDef
                  string.Compare(fieldName, "deathMessage", true) == 0 ||

                  // HediffDef
                  string.Compare(fieldName, "discoverLetterLabel", true) == 0 ||
                  string.Compare(fieldName, "discoverLetterText", true) == 0 ||
                  string.Compare(fieldName, "labelTendedWell", true) == 0 ||
                  string.Compare(fieldName, "labelTended", true) == 0 ||
                  string.Compare(fieldName, "labelTendedWellInner", true) == 0 ||
                  string.Compare(fieldName, "labelTendedInner", true) == 0 ||
                  string.Compare(fieldName, "labelSolidTendedWell", true) == 0 ||
                  string.Compare(fieldName, "labelSolidTended", true) == 0 ||
                  string.Compare(fieldName, "destroyedLabel", true) == 0 ||
                  string.Compare(fieldName, "destroyedOutLabel", true) == 0 ||
                  string.Compare(fieldName, "oldLabel", true) == 0 ||
                  string.Compare(fieldName, "instantlyOldLabel", true) == 0 ||

                  // InspirationDef
                  string.Compare(fieldName, "beginLetterLabel", true) == 0 ||
                  string.Compare(fieldName, "beginLetter", true) == 0 ||
                  string.Compare(fieldName, "endMessage", true) == 0 ||
                  string.Compare(fieldName, "baseInspectLine", true) == 0 ||
                  
                  // JobDef
                  string.Compare(fieldName, "reportString", true) == 0 ||

                  // MapConditionDef
                  //string.Compare(fieldName, "endMessage", true) == 0 ||

                  // MentalStateDef
                  //string.Compare(fieldName, "beginLetterLabel", true) == 0 ||
                  //string.Compare(fieldName, "beginLetter", true) == 0 ||
                  string.Compare(fieldName, "recoveryMessage", true) == 0 ||
                  //string.Compare(fieldName, "baseInspectLine", true) == 0 ||

                  // PawnCapacityDef
                  string.Compare(fieldName, "labelMechanoids", true) == 0 ||

                  // PawnKindDef
                  string.Compare(fieldName, "labelPlural", true) == 0 ||
                  string.Compare(fieldName, "labelMale", true) == 0 ||
                  string.Compare(fieldName, "labelMalePlural", true) == 0 ||
                  string.Compare(fieldName, "labelFemale", true) == 0 ||
                  string.Compare(fieldName, "labelFemalePlural", true) == 0 ||

                  // RecipeDef
                  string.Compare(fieldName, "jobString", true) == 0 ||
                  string.Compare(fieldName, "successfullyRemovedHediffMessage", true) == 0 ||

                  // ResearchProjectDef
                  string.Compare(fieldName, "descriptionDiscovered", true) == 0 ||


                  // ThingDef
                  string.Compare(fieldName, "stuffAdjective", true) == 0 ||
                  string.Compare(fieldName, "useLabel", true) == 0 ||
                  string.Compare(fieldName, "ingestCommandString", true) == 0 ||
                  string.Compare(fieldName, "ingestReportString", true) == 0 ||
                  string.Compare(fieldName, "offMessage", true) == 0 ||

                  // WorkTypeDef
                  string.Compare(fieldName, "labelShort", true) == 0 ||
                  //string.Compare(fieldName, "pawnLabel", true) == 0 ||
                  string.Compare(fieldName, "gerundLabel", true) == 0 ||
                  //string.Compare(fieldName, "verb", true) == 0 ||

                  // RimTrans.Framework
                  string.Compare(fieldName, "Title", true) == 0 ||
                  string.Compare(fieldName, "TitleShort", true) == 0 ||
                  string.Compare(fieldName, "BaseDesc", true) == 0 ||
                  string.Compare(fieldName, "First", true) == 0 ||
                  string.Compare(fieldName, "Last", true) == 0 ||
                  string.Compare(fieldName, "Nick", true) == 0 ||
                  
                  // Support for Rimatomics
                  string.Compare(fieldName, "ResearchLabel") == 0 ||
                  string.Compare(fieldName, "ResearchDesc") == 0 ||
                  string.Compare(fieldName, "ResearchDescDisc") == 0 ||
                  string.Compare(fieldName, "StepLabel") == 0 ||
                  string.Compare(fieldName, "StepDesc") == 0 ||

                  // Support for AlienRaces
                  string.Compare(fieldName, "baseDescription") == 0 ||
                  string.Compare(fieldName, "title") == 0 ||
                  string.Compare(fieldName, "titleShort") == 0 ||
                  string.Compare(fieldName, "first") == 0 ||
                  string.Compare(fieldName, "last") == 0 ||
                  string.Compare(fieldName, "nick") == 0 

                  ) {
                return true;
            }
            return false;
        }

        #endregion

        #region Judge Injectable Field Extra

        private static List<string> InjectableFieldNamesExtra;

        public static void ReadExtraFieldNames(string path) {
            InjectableFieldNamesExtra = null;
            using (StreamReader sr = new StreamReader(path)) {
                List<string> fieldNames = new List<string>();
                while (sr.Peek() >= 0) {
                    string fieldName = sr.ReadLine().Trim();
                    if (fieldName != string.Empty) {
                        fieldNames.Add(fieldName);
                    }
                }
                if (fieldNames.Count > 0) {
                    InjectableFieldNamesExtra = fieldNames;
                }
            }
        }

        private static List<string> InjectableFieldNamesParentExtra;

        public static void ReadExtraFieldNamesParent(string path) {
            InjectableFieldNamesParentExtra = null;
            using (StreamReader sr = new StreamReader(path)) {
                List<string> fieldNames = new List<string>();
                while (sr.Peek() >= 0) {
                    string fieldName = sr.ReadLine().Trim();
                    if (fieldName != string.Empty) {
                        fieldNames.Add(fieldName);
                    }
                }
                if (fieldNames.Count > 0) {
                    InjectableFieldNamesParentExtra = fieldNames;
                }
            }
        }

        public static void CleanExtra() {
            InjectableFieldNamesExtra = null;
            InjectableFieldNamesParentExtra = null;
        }

        public static bool IsInjectableExtra(this XElement field) {
            string fieldName = field.Name.ToString();
            if (fieldName == "li" && InjectableFieldNamesParentExtra != null) {
                string fieldNameParent = field.Parent.Name.ToString();
                foreach (string injectableFieldNameParent in InjectableFieldNamesParentExtra) {
                    if (fieldNameParent == injectableFieldNameParent) {
                        return true;
                    }
                }
            } else if (fieldName != "li" && InjectableFieldNamesExtra != null) {
                foreach (string injectableFieldName in InjectableFieldNamesExtra) {
                    if (fieldName == injectableFieldName) {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Debug

        public static void Debug() {
            if (InjectableFieldNamesExtra != null) {
                foreach (string fieldName in InjectableFieldNamesExtra) {
                    Console.WriteLine(fieldName);
                }
            }
            if (InjectableFieldNamesParentExtra != null) {
                foreach (string fieldName in InjectableFieldNamesParentExtra) {
                    Console.WriteLine(fieldName);
                }
            }
        }

        #endregion
    }
}
