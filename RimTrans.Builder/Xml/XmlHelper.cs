using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

        #region Judge Injectable Def

        public static bool IsNeedToTranslate(this XElement def)
        {
            string defTypeName = def.Name.ToString();
            if (defTypeName == "Def" ||
                defTypeName == "BuildableDef" ||
                defTypeName == "DesignationDef" ||
                defTypeName == "DutyDef" ||
                defTypeName == "EffecterDef" ||
                defTypeName == "GenStepDef" ||
                defTypeName == "JoyGiverDef" ||
                defTypeName == "MapGeneratorDef" ||
                defTypeName == "MentalBreakDef" ||
                defTypeName == "PawnGroupKindDef" ||
                defTypeName == "RuleDef" ||
                defTypeName == "ScatterableDef" ||
                defTypeName == "SongDef" ||
                defTypeName == "SoundDef" ||
                defTypeName == "ThinkTreeDef")
            {
                return false;
            }
            return true;
        }

        #endregion

        #region Judge Injectable Field

        public static bool IsInjectable(this XElement field)
        {
            string fieldName = field.Name.ToString();
            if (fieldName == "li")
            {
                string fieldNameParent = field.Parent.Name.ToString();
                if (fieldNameParent == "rulesStrings")
                {
                    return true;
                }
            }
            else if (
                // Def
                fieldName == "label" ||
                fieldName == "description" ||

                // ConceptDef
                fieldName == "helpText" ||

                // FactionDef
                fieldName == "pawnsPlural" ||
                fieldName == "fixedName" ||
                fieldName == "leaderTitle" ||

                // HediffGiverSetDef
                fieldName == "letterLabel" ||
                fieldName == "letter" ||

                // HistoryAutoRecorderGroupDef
                fieldName == "graphLabelY" ||

                // IncidentDef
                //fieldName == "letterLabel" ||
                fieldName == "letterText" ||

                // InstructionDef
                fieldName == "text" ||
                fieldName == "rejectInputMessage" ||
                fieldName == "onMapInstruction" ||

                // LifeStageDef
                fieldName == "adjective" ||

                // PawnRelationDef
                //fieldName == "labelFemale" ||

                // RaidStrategyDef
                fieldName == "letterLabelEnemy" ||
                fieldName == "arrivalTextEnemy" ||
                fieldName == "letterLabelFriendly" ||
                fieldName == "arrivalTextFriendly" ||

                // ScenarioDef
                fieldName == "name" ||
                fieldName == "summary" ||
                //fieldName == "text" ||

                // SkillDef
                fieldName == "skillLabel" ||
                fieldName == "pawnLabel" ||

                // StatDef
                fieldName == "customLabel" ||
                fieldName == "formatString" ||

                // ThoughtDef
                fieldName == "labelSocial" ||

                // WorkGiverDef
                fieldName == "verb" ||
                fieldName == "gerund" ||

                // Verse

                // DamageDef
                fieldName == "deathMessage" ||

                // HediffDef
                fieldName == "discoverLetterLabel" ||
                fieldName == "discoverLetterText" ||
                fieldName == "labelTendedWell" ||
                fieldName == "labelTended" ||
                fieldName == "labelTendedWellInner" ||
                fieldName == "labelTendedInner" ||
                fieldName == "labelSolidTendedWell" ||
                fieldName == "labelSolidTended" ||
                fieldName == "destroyedLabel" ||
                fieldName == "oldLabel" ||
                fieldName == "destroyedOutLabel" ||
                fieldName == "instantlyOldLabel" ||

                // JobDef
                fieldName == "reportString" ||

                // MapConditionDef
                fieldName == "endMessage" ||

                // MentalStateDef
                fieldName == "beginLetterLabel" ||
                fieldName == "beginLetter" ||
                fieldName == "recoveryMessage" ||
                fieldName == "baseInspectLine" ||

                // PawnCapacityDef
                fieldName == "labelMechanoids" ||

                // PawnKindDef
                fieldName == "labelPlural" ||
                fieldName == "labelMale" ||
                fieldName == "labelMalePlural" ||
                fieldName == "labelFemale" ||
                fieldName == "labelFemalePlural" ||

                // RecipeDef
                fieldName == "jobString" ||
                fieldName == "successfullyRemovedHediffMessage" ||

                // ResearchProjectDef
                fieldName == "descriptionDiscovered" ||


                // ThingDef
                fieldName == "stuffAdjective" ||
                fieldName == "useLabel" ||
                fieldName == "ingestCommandString" ||
                fieldName == "ingestReportString" ||

                // WorkTypeDef
                fieldName == "labelShort" ||
                //fieldName == "pawnLabel" ||
                fieldName == "gerundLabel"
                //fieldName == "verb" ||
                )
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Judge Injectable Field Extra

        private static List<string> InjectableFieldNamesExtra;

        public static void ReadExtraFieldNames(string path)
        {
            InjectableFieldNamesExtra = null;
            using (StreamReader sr = new StreamReader(path))
            {
                List<string> fieldNames = new List<string>();
                while (sr.Peek() >= 0)
                {
                    string fieldName = sr.ReadLine().Trim();
                    if (fieldName != string.Empty)
                    {
                        fieldNames.Add(fieldName);
                    }
                }
                if (fieldNames.Count > 0)
                {
                    InjectableFieldNamesExtra = fieldNames;
                }
            }
        }

        private static List<string> InjectableFieldNamesParentExtra;

        public static void ReadExtraFieldNamesParent(string path)
        {
            InjectableFieldNamesParentExtra = null;
            using (StreamReader sr = new StreamReader(path))
            {
                List<string> fieldNames = new List<string>();
                while (sr.Peek() >= 0)
                {
                    string fieldName = sr.ReadLine().Trim();
                    if (fieldName != string.Empty)
                    {
                        fieldNames.Add(fieldName);
                    }
                }
                if (fieldNames.Count > 0)
                {
                    InjectableFieldNamesParentExtra = fieldNames;
                }
            }
        }

        public static void CleanExtra()
        {
            InjectableFieldNamesExtra = null;
            InjectableFieldNamesParentExtra = null;
        }

        public static bool IsInjectableExtra(this XElement field)
        {
            string fieldName = field.Name.ToString();
            if (fieldName == "li" && InjectableFieldNamesParentExtra != null)
            {
                string fieldNameParent = field.Parent.Name.ToString();
                foreach (string injectableFieldNameParent in InjectableFieldNamesParentExtra)
                {
                    if (fieldNameParent == injectableFieldNameParent)
                    {
                        return true;
                    }
                }
            }
            else if (fieldName != "li" && InjectableFieldNamesExtra != null)
            {
                foreach (string injectableFieldName in InjectableFieldNamesExtra)
                {
                    if (fieldName == injectableFieldName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        #endregion

        #region Debug

        public static void Debug()
        {
            if (InjectableFieldNamesExtra != null)
            {
                foreach (string fieldName in InjectableFieldNamesExtra)
                {
                    Console.WriteLine(fieldName);
                }
            }
            if (InjectableFieldNamesParentExtra != null)
            {
                foreach (string fieldName in InjectableFieldNamesParentExtra)
                {
                    Console.WriteLine(fieldName);
                }
            }
        }

        #endregion
    }
}
