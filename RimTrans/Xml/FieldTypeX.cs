using System;
using System.Linq;
using System.Xml.Linq;

namespace RimTrans.Xml
{
    public static class FieldTypeX
    {
        public static FieldType GetFieldType(this XElement field)
        {
            FieldType fieldType;
            Enum.TryParse<FieldType>(field.Name.ToString(), true, out fieldType); // Ignore Case
            return fieldType;
        }

        //
        public static bool IsInjectableField(this XElement field)
        {
            FieldType fieldType = field.GetFieldType();
            switch (fieldType)
            {
                // RimWorld

                // Def
                case FieldType.label:
                case FieldType.description:

                // ConceptDef
                case FieldType.helpText:

                // FactionDef
                case FieldType.pawnsPlural:
                case FieldType.fixedName:
                case FieldType.leaderTitle:

                // HairDef
                case FieldType.hairGender:

                // HediffGiverSetDef
                case FieldType.letterLabel:
                case FieldType.letter:

                // HistoryAutoRecorderGroupDef
                case FieldType.graphLabelY:

                // IncidentDef
                //case FieldType.letterLabel:
                case FieldType.letterText:

                // InstructionDef
                case FieldType.text:
                case FieldType.rejectInputMessage:
                case FieldType.onMapInstruction:

                // LifeStageDef
                case FieldType.adjective:

                // PawnRelationDef
                //case FieldType.labelFemale:

                // RaidStrategyDef
                case FieldType.letterLabelEnemy:
                case FieldType.arrivalTextEnemy:
                case FieldType.letterLabelFriendly:
                case FieldType.arrivalTextFriendly:

                // ScenarioDef
                case FieldType.name:
                case FieldType.summary:
                //case FieldType.text:

                // SkillDef
                case FieldType.skillLabel:
                case FieldType.pawnLabel:

                // StatDef
                case FieldType.customLabel:
                case FieldType.formatString:

                // ThoughtDef
                case FieldType.labelSocial:

                // WorkGiverDef
                case FieldType.verb:
                case FieldType.gerund:

                // Verse

                // DamageDef
                case FieldType.deathMessage:

                // HediffDef
                case FieldType.discoverLetterLabel:
                case FieldType.discoverLetterText:
                case FieldType.labelTendedWell:
                case FieldType.labelTended:
                case FieldType.labelTendedWellInner:
                case FieldType.labelTendedInner:
                case FieldType.labelSolidTendedWell:
                case FieldType.labelSolidTended:
                case FieldType.destroyedLabel:
                case FieldType.oldLabel:
                case FieldType.destroyedOutLabel:
                case FieldType.instantlyOldLabel:

                // JobDef
                case FieldType.reportString:

                // MapConditionDef
                case FieldType.endMessage:

                // MentalStateDef
                case FieldType.beginLetterLabel:
                case FieldType.beginLetter:
                case FieldType.recoveryMessage:
                case FieldType.baseInspectLine:

                // PawnCapacityDef
                case FieldType.labelMechanoids:

                // PawnKindDef
                case FieldType.labelMale:
                case FieldType.labelFemale:

                // RecipeDef
                case FieldType.jobString:
                case FieldType.successfullyRemovedHediffMessage:

                // ResearchProjectDef
                case FieldType.descriptionDiscovered:


                // ThingDef
                case FieldType.stuffAdjective:
                case FieldType.useLabel:
                case FieldType.ingestCommandString:
                case FieldType.ingestReportString:

                // WorkTypeDef
                case FieldType.labelShort:
                //case FieldType.pawnLabel:
                case FieldType.gerundLabel:
                //case FieldType.verb:

                    return true;

                // InteractionDef TaleDef RulePackDef
                case FieldType.li:
                    FieldType parentFieldType = field.Parent.GetFieldType();
                    if (parentFieldType == FieldType.rulesStrings)
                        return true;
                    else
                        return false;

                default:
                    return false;
            }
        }
    }
}
