using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimTrans.Builder.Xml {
    public static class DefTypeNameOf {
        private static IEnumerable<string> getAllNames() {
            Type typeDef = typeof(Def);
            yield return typeDef.Name;
            foreach (Type subclass in typeDef.AllSubclasses()) {
                yield return subclass.Name;
            }
        }
        private static readonly List<string> allNames = getAllNames().ToList();

        public static IEnumerable<string> AllNames => allNames;

        //public static readonly string Unknown = "Unknown";
        public static readonly string Def = "Def";
        public static readonly string BillRepeatModeDef = "BillRepeatModeDef";
        public static readonly string BillStoreModeDef = "BillStoreModeDef";
        public static readonly string BiomeDef = "BiomeDef";
        public static readonly string ChemicalDef = "ChemicalDef";
        public static readonly string DifficultyDef = "DifficultyDef";
        public static readonly string FactionDef = "FactionDef";
        public static readonly string FeatureDef = "FeatureDef";
        public static readonly string FleshTypeDef = "FleshTypeDef";
        public static readonly string HairDef = "HairDef";
        public static readonly string HediffGiverSetDef = "HediffGiverSetDef";
        public static readonly string HibernatableStateDef = "HibernatableStateDef";
        public static readonly string HistoryAutoRecorderDef = "HistoryAutoRecorderDef";
        public static readonly string HistoryAutoRecorderGroupDef = "HistoryAutoRecorderGroupDef";
        public static readonly string IncidentDef = "IncidentDef";
        public static readonly string IncidentTargetTypeDef = "IncidentTargetTypeDef";
        public static readonly string InspirationDef = "InspirationDef";
        public static readonly string InteractionDef = "InteractionDef";
        public static readonly string ItemCollectionGeneratorDef = "ItemCollectionGeneratorDef";
        public static readonly string JoyGiverDef = "JoyGiverDef";
        public static readonly string JoyKindDef = "JoyKindDef";
        public static readonly string LifeStageDef = "LifeStageDef";
        public static readonly string MainButtonDef = "MainButtonDef";
        public static readonly string NeedDef = "NeedDef";
        public static readonly string PawnColumnDef = "PawnColumnDef";
        public static readonly string PawnGroupKindDef = "PawnGroupKindDef";
        public static readonly string PawnRelationDef = "PawnRelationDef";
        public static readonly string PawnTableDef = "PawnTableDef";
        public static readonly string PrisonerInteractionModeDef = "PrisonerInteractionModeDef";
        public static readonly string RaidStrategyDef = "RaidStrategyDef";
        public static readonly string RecordDef = "RecordDef";
        public static readonly string ResearchTabDef = "ResearchTabDef";
        public static readonly string RiverDef = "RiverDef";
        public static readonly string RoadDef = "RoadDef";
        public static readonly string RoadPathingDef = "RoadPathingDef";
        public static readonly string RoadWorldLayerDef = "RoadWorldLayerDef";
        public static readonly string RuleDef = "RuleDef";
        public static readonly string ScenPartDef = "ScenPartDef";
        public static readonly string ScenarioDef = "ScenarioDef";
        public static readonly string SiteCoreDef = "SiteCoreDef";
        public static readonly string SiteDefBase = "SiteDefBase";
        public static readonly string SitePartDef = "SitePartDef";
        public static readonly string SkillDef = "SkillDef";
        public static readonly string StatCategoryDef = "StatCategoryDef";
        public static readonly string StatDef = "StatDef";
        public static readonly string StoryEventDef = "StoryEventDef";
        public static readonly string StorytellerDef = "StorytellerDef";
        public static readonly string StuffCategoryDef = "StuffCategoryDef";
        public static readonly string TaleDef = "TaleDef";
        public static readonly string ThoughtDef = "ThoughtDef";
        public static readonly string TimeAssignmentDef = "TimeAssignmentDef";
        public static readonly string TraderKindDef = "TraderKindDef";
        public static readonly string TrainableDef = "TrainableDef";
        public static readonly string TraitDef = "TraitDef";
        public static readonly string TransferableSorterDef = "TransferableSorterDef";
        public static readonly string ConceptDef = "ConceptDef";
        public static readonly string InstructionDef = "InstructionDef";
        public static readonly string WorldObjectDef = "WorldObjectDef";
        public static readonly string BodyDef = "BodyDef";
        public static readonly string BodyPartDef = "BodyPartDef";
        public static readonly string BodyPartGroupDef = "BodyPartGroupDef";
        public static readonly string BuildableDef = "BuildableDef";
        public static readonly string DamageArmorCategoryDef = "DamageArmorCategoryDef";
        public static readonly string DamageDef = "DamageDef";
        public static readonly string StuffAppearanceDef = "StuffAppearanceDef";
        public static readonly string DesignationCategoryDef = "DesignationCategoryDef";
        public static readonly string DesignationDef = "DesignationDef";
        public static readonly string DutyDef = "DutyDef";
        public static readonly string EffecterDef = "EffecterDef";
        public static readonly string GameConditionDef = "GameConditionDef";
        public static readonly string GenStepDef = "GenStepDef";
        public static readonly string HediffDef = "HediffDef";
        public static readonly string ImpactSoundTypeDef = "ImpactSoundTypeDef";
        public static readonly string ImplementOwnerTypeDef = "ImplementOwnerTypeDef";
        public static readonly string JobDef = "JobDef";
        public static readonly string KeyBindingCategoryDef = "KeyBindingCategoryDef";
        public static readonly string KeyBindingDef = "KeyBindingDef";
        public static readonly string LetterDef = "LetterDef";
        public static readonly string ManeuverDef = "ManeuverDef";
        public static readonly string MapGeneratorDef = "MapGeneratorDef";
        public static readonly string MentalBreakDef = "MentalBreakDef";
        public static readonly string MentalStateDef = "MentalStateDef";
        public static readonly string MessageTypeDef = "MessageTypeDef";
        public static readonly string PawnCapacityDef = "PawnCapacityDef";
        public static readonly string PawnKindDef = "PawnKindDef";
        public static readonly string RecipeDef = "RecipeDef";
        public static readonly string ResearchProjectDef = "ResearchProjectDef";
        public static readonly string ReservationLayerDef = "ReservationLayerDef";
        public static readonly string RoofDef = "RoofDef";
        public static readonly string RoomRoleDef = "RoomRoleDef";
        public static readonly string RoomStatDef = "RoomStatDef";
        public static readonly string RulePackDef = "RulePackDef";
        public static readonly string ScatterableDef = "ScatterableDef";
        public static readonly string SongDef = "SongDef";
        public static readonly string SoundDef = "SoundDef";
        public static readonly string SubcameraDef = "SubcameraDef";
        public static readonly string TerrainDef = "TerrainDef";
        public static readonly string ThingCategoryDef = "ThingCategoryDef";
        public static readonly string ThingDef = "ThingDef";
        public static readonly string ThinkTreeDef = "ThinkTreeDef";
        public static readonly string ToolCapacityDef = "ToolCapacityDef";
        public static readonly string TrainableIntelligenceDef = "TrainableIntelligenceDef";
        public static readonly string WeatherDef = "WeatherDef";
        public static readonly string WorkGiverDef = "WorkGiverDef";
        public static readonly string WorkTypeDef = "WorkTypeDef";
        public static readonly string WorldGenStepDef = "WorldGenStepDef";
        public static readonly string SpecialThingFilterDef = "SpecialThingFilterDef";

        // Before A18
        public static readonly string DrawTargetDef = "DrawTargetDef";
        public static readonly string SectionLayerPhaseDef = "SectionLayerPhaseDef";

        // Before A17
        public static readonly string MapConditionDef = "MapConditionDef";
        public static readonly string MainTabDef = "MainTabDef";
    }
}
