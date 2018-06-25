/**
 * Scan fields label and description in all Defs.
 */

/**
 * Test definition.ts
 */

import fs from 'fs';
import globby from 'globby';
import { readRawContents } from './utils';
import init from './env-init';
import { RawContents, stringCompare } from '../core/utils';
import * as xml from '../core/xml';
import * as definition from '../core/definition';

const { dirCore } = init();

const map: { [defType: string]: boolean } = {};

async function test(): Promise<void> {
  const rawContents: RawContents = await readRawContents(`${dirCore}/Defs/**/*.xml`);

  const dataOrigin: definition.DefinitionData = {};

  Object.entries(rawContents).forEach(([path, content]) => {
    const root: xml.Element = xml.parse(content);
    root.nodes.filter(xml.isElement).forEach(def => {
      if (!dataOrigin[def.name]) {
        dataOrigin[def.name] = [];
      }
      dataOrigin[def.name].push(def);
    });
  });

  Object.entries(dataOrigin).forEach(([defType, defs]) => {
    for (const def of defs) {
      if (
        def.nodes.some(xml.isElementByName('label')) ||
        def.nodes.some(xml.isElementByName('description'))
      ) {
        map[defType] = true;

        return;
      }
    }

    map[defType] = false;
  });

  Object.entries(map)
    .sort((a, b) => stringCompare(a[0], b[0]))
    .forEach(([defType, value]) => console.log(`${defType}: ${value},`));
}

test().catch(error => console.log(error));

export default {
  ApparelLayerDef: true,
  BillRepeatModeDef: true,
  BillStoreModeDef: true,
  BiomeDef: true,
  BodyDef: true,
  BodyPartDef: true,
  BodyPartGroupDef: true,
  BodyPartTagDef: false,
  BodyTypeDef: false,
  ChemicalDef: true,
  ClamorDef: false,
  ConceptDef: true,
  DamageArmorCategoryDef: false,
  DamageDef: true,
  DesignationCategoryDef: true,
  DesignationDef: false,
  DesignatorDropdownGroupDef: false,
  DifficultyDef: true,
  DutyDef: false,
  EffecterDef: false,
  FactionDef: true,
  FeatureDef: false,
  FleshTypeDef: false,
  GameConditionDef: true,
  GenStepDef: false,
  HairDef: true,
  HediffDef: true,
  HediffGiverSetDef: false,
  HibernatableStateDef: false,
  HistoryAutoRecorderDef: true,
  HistoryAutoRecorderGroupDef: true,
  ImpactSoundTypeDef: false,
  ImplementOwnerTypeDef: false,
  IncidentCategoryDef: false,
  IncidentDef: true,
  IncidentTargetTypeDef: false,
  InspirationDef: true,
  InstructionDef: false,
  InteractionDef: true,
  JobDef: false,
  JoyGiverDef: false,
  JoyKindDef: true,
  KeyBindingCategoryDef: true,
  KeyBindingDef: true,
  LetterDef: false,
  LifeStageDef: true,
  LogEntryDef: false,
  MainButtonDef: true,
  ManeuverDef: false,
  MapGeneratorDef: false,
  MentalBreakDef: true,
  MentalStateDef: true,
  MessageTypeDef: false,
  NeedDef: true,
  PawnCapacityDef: true,
  PawnColumnDef: true,
  PawnGroupKindDef: false,
  PawnKindDef: true,
  PawnRelationDef: true,
  PawnsArrivalModeDef: false,
  PawnTableDef: false,
  PrisonerInteractionModeDef: true,
  RaidStrategyDef: false,
  RecipeDef: true,
  RecordDef: true,
  ResearchProjectDef: true,
  ResearchProjectTagDef: false,
  ResearchTabDef: true,
  ReservationLayerDef: false,
  RiverDef: true,
  RoadDef: true,
  RoadPathingDef: false,
  RoadWorldLayerDef: false,
  RoofDef: true,
  RoomRoleDef: true,
  RoomStatDef: true,
  RuleDef: false,
  RulePackDef: false,
  ScatterableDef: false,
  ScenarioDef: true,
  ScenPartDef: true,
  ShaderTypeDef: false,
  SiteCoreDef: true,
  SitePartDef: true,
  SkillDef: true,
  SongDef: false,
  SoundDef: false,
  SpecialThingFilterDef: true,
  StatCategoryDef: true,
  StatDef: true,
  StoryEventDef: false,
  StorytellerDef: true,
  StuffAppearanceDef: false,
  StuffCategoryDef: true,
  SubcameraDef: false,
  TaleDef: true,
  TerrainAffordanceDef: true,
  TerrainDef: true,
  ThingCategoryDef: true,
  ThingDef: true,
  ThingSetMakerDef: false,
  ThinkTreeDef: false,
  ThoughtDef: false,
  TimeAssignmentDef: true,
  ToolCapacityDef: false,
  TraderKindDef: true,
  TrainabilityDef: true,
  TrainableDef: true,
  TraitDef: false,
  TransferableSorterDef: true,
  WeatherDef: true,
  WorkGiverDef: true,
  WorkGiverEquivalenceGroupDef: false,
  WorkTypeDef: true,
  WorldGenStepDef: false,
  WorldObjectDef: true,
};
