using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace RimTrans.Builder
{
    public static class RWXml
    {
        #region DefType, FieldName and ThingCategory

        public static RWDefType GetDefType(XElement def)
        {
            RWDefType defType = RWDefType.Unknown;
            try
            {
                defType = (RWDefType)Enum.Parse(typeof(RWDefType), def.Name.ToString()); // Ignore Case
            }
            catch (Exception)
            {
            }
            return defType;
        }

        public static RWFieldName GetFieldName(XElement field)
        {
            RWFieldName fieldName = RWFieldName.Unknown;
            try
            {
                fieldName =(RWFieldName)Enum.Parse(typeof(RWFieldName), field.Name.ToString(), true); // Ignore Case
            }
            catch (Exception)
            {
            }
            return fieldName;
        }

        public static RWThingCategory GetCategory(XElement def)
        {
            RWThingCategory thingCategory = RWThingCategory.None;
            XElement category = RWXml.GetField(def, RWFieldName.category);
            if (category != null)
            {
                try
                {
                    thingCategory = (RWThingCategory)Enum.Parse(typeof(RWThingCategory), category.Value, true); // Ignore Case
                }
                catch (Exception)
                {
                }
            }
            return thingCategory;
        }

        public static bool IsInjectable(XElement field)
        {
            RWFieldName fieldType = RWXml.GetFieldName(field);
            switch (fieldType)
            {
                // RimWorld

                // Def
                case RWFieldName.label:
                case RWFieldName.description:

                // ConceptDef
                case RWFieldName.helpText:

                // FactionDef
                case RWFieldName.pawnsPlural:
                case RWFieldName.fixedName:
                case RWFieldName.leaderTitle:

                // HediffGiverSetDef
                case RWFieldName.letterLabel:
                case RWFieldName.letter:

                // HistoryAutoRecorderGroupDef
                case RWFieldName.graphLabelY:

                // IncidentDef
                //case FieldType.letterLabel:
                case RWFieldName.letterText:

                // InstructionDef
                case RWFieldName.text:
                case RWFieldName.rejectInputMessage:
                case RWFieldName.onMapInstruction:

                // LifeStageDef
                case RWFieldName.adjective:

                // PawnRelationDef
                //case FieldType.labelFemale:

                // RaidStrategyDef
                case RWFieldName.letterLabelEnemy:
                case RWFieldName.arrivalTextEnemy:
                case RWFieldName.letterLabelFriendly:
                case RWFieldName.arrivalTextFriendly:

                // ScenarioDef
                case RWFieldName.name:
                case RWFieldName.summary:
                //case FieldType.text:

                // SkillDef
                case RWFieldName.skillLabel:
                case RWFieldName.pawnLabel:

                // StatDef
                case RWFieldName.customLabel:
                case RWFieldName.formatString:

                // ThoughtDef
                case RWFieldName.labelSocial:

                // WorkGiverDef
                case RWFieldName.verb:
                case RWFieldName.gerund:

                // Verse

                // DamageDef
                case RWFieldName.deathMessage:

                // HediffDef
                case RWFieldName.discoverLetterLabel:
                case RWFieldName.discoverLetterText:
                case RWFieldName.labelTendedWell:
                case RWFieldName.labelTended:
                case RWFieldName.labelTendedWellInner:
                case RWFieldName.labelTendedInner:
                case RWFieldName.labelSolidTendedWell:
                case RWFieldName.labelSolidTended:
                case RWFieldName.destroyedLabel:
                case RWFieldName.oldLabel:
                case RWFieldName.destroyedOutLabel:
                case RWFieldName.instantlyOldLabel:

                // JobDef
                case RWFieldName.reportString:

                // MapConditionDef
                case RWFieldName.endMessage:

                // MentalStateDef
                case RWFieldName.beginLetterLabel:
                case RWFieldName.beginLetter:
                case RWFieldName.recoveryMessage:
                case RWFieldName.baseInspectLine:

                // PawnCapacityDef
                case RWFieldName.labelMechanoids:

                // PawnKindDef
                case RWFieldName.labelMale:
                case RWFieldName.labelFemale:

                // RecipeDef
                case RWFieldName.jobString:
                case RWFieldName.successfullyRemovedHediffMessage:

                // ResearchProjectDef
                case RWFieldName.descriptionDiscovered:


                // ThingDef
                case RWFieldName.stuffAdjective:
                case RWFieldName.useLabel:
                case RWFieldName.ingestCommandString:
                case RWFieldName.ingestReportString:

                // WorkTypeDef
                case RWFieldName.labelShort:
                //case FieldType.pawnLabel:
                case RWFieldName.gerundLabel:
                    //case FieldType.verb:

                    return true;

                // InteractionDef TaleDef RulePackDef
                case RWFieldName.li:
                    RWFieldName parentFieldType = RWXml.GetFieldName(field.Parent);
                    if (parentFieldType == RWFieldName.rulesStrings ||
                        parentFieldType == RWFieldName.helpTexts /* for version earlier than A15 */)
                        return true;
                    else
                        return false;

                default:
                    return false;
            }
        }

        #endregion

        /// <summary>
        /// Get the child element in this element by fieldName
        /// </summary>
        /// <param name="element"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static XElement GetField(XElement element, RWFieldName fieldName)
        {
            XElement result = null;
            foreach (XElement field in element.Elements())
            {
                if (string.Compare(field.Name.ToString(), fieldName.ToString(), true) == 0)
                //if (RWXml.GetFieldName(field) == fieldName)
                {
                    result = field;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// If the Def has &lt;defName&gt; fields.
        /// </summary>
        public static bool HasDefName(XElement def)
        {
            bool result = false;
            foreach (XElement field in def.Elements())
            {
                if (string.Compare(field.Name.ToString(), "defName", true) == 0)
                //if (RWXml.GetFieldName(field) == RWFieldName.defName)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Compare the names of two injection fields
        /// </summary>
        public static bool CompareFieldsName(XElement fieldA, XElement fieldB)
        {
            string fieldNameA = fieldA.Name.ToString();
            string fieldNameB = fieldB.Name.ToString();
            int defNameLength = 0;
            if (string.Compare(fieldNameA, fieldNameB, true) == 0)
            {
                defNameLength = fieldNameA.IndexOf('.');
            }
            return defNameLength > 0 && fieldNameA.Substring(0, defNameLength) == fieldNameB.Substring(0, defNameLength);
        }



        public static XDocument EmptyDocument()
        {
            return XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<LanguageData>\r\n\r\n</LanguageData>", LoadOptions.PreserveWhitespace);
        }

        public static XDocument EmptyDocumentSpecial()
        {
            return XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<LanguageData LastIsSingle=\"false\">\r\n\r\n</LanguageData>", LoadOptions.PreserveWhitespace);
        }

        public static XDocument LoadLanguageDocument(string filePath)
        {
            XDocument doc = XDocument.Load(filePath, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri);
            if (TransOption.IsResetIndentWhenLoad)
            {
                foreach (XText text in from node in doc.Root.Nodes()
                                       where node.NodeType == XmlNodeType.Text
                                       select node)
                {
                    text.Value = text.Value.Replace(" ", string.Empty).Replace("\t", string.Empty) + TransOption.Indent;
                }
                XNode nodeLast = doc.Root.LastNode;
                if (nodeLast.NodeType == XmlNodeType.Text)
                {
                    (nodeLast as XText).Value = (nodeLast as XText).Value.Replace(" ", string.Empty);
                }
            }
            return doc;
        }

        /// <summary>
        /// Comment all of the elements in the invalid document
        /// </summary>
        public static XDocument DocumentCommentAll(XDocument documentInvalid)
        {
            XDocument documentNew = new XDocument(documentInvalid);
            while (documentNew.Root.Elements().Count() > 0)
            {
                XElement field = documentNew.Root.Elements().First();
                field.ReplaceWith(new XComment(field.ToString()));
            }
            return documentNew;
        }
    }
}
