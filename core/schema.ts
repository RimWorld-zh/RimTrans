import { POINT_CONVERSION_UNCOMPRESSED } from 'constants';

// tslint:disable:typedef variable-name

// ======== Enum ========

export enum SchemaType {
  NoTranslate,
  Def,
  Special,
  MustTranslate,
  MayTranslate,
  TranslationCanChangeCount,
}

// ======== some abstract def type =======

const BuildableDef = {};

const SiteDefBase = {
  approachOrderString: true,
  approachingReportString: true,
  arrivedLetter: true,
  arrivedLetterLabel: true,
};

// ======== some properties type ========

const VerbProperties = {
  label: true,
};

const Tool = {
  label: true,
};

// BodyDef

// HACK: recursive
const BodyPartRecord = {
  customLabel: true,
};

// FactionDef

const ThingFilter = {
  customSummary: true,
};

// HediffDef HediffGiverSetDef

const HediffCompProperties_Discoverable = {
  discoverLetterLabel: true,
  discoverLetterText: true,
};
const HediffCompProperties_GetsPermanent = {
  permanentLabel: true,
  instantlyPermanentLabel: true,
};
const HediffCompProperties_TendDuration = {
  labelTreatedWell: true,
  labelTendedWell: true,
  labelTreatedWellInner: true,
  labelTendedWellInner: true,
  labelSolidTreatedWell: true,
  labelSolidTendedWell: true,
};
const HediffCompProperties_VerbGiver = {
  verbs: {
    li: {
      ...VerbProperties,
    },
  },
  tools: {
    li: {
      ...Tool,
    },
  },
};
const HediffCompProperties = {
  ...HediffCompProperties_Discoverable,
  ...HediffCompProperties_GetsPermanent,
  ...HediffCompProperties_TendDuration,
  ...HediffCompProperties_VerbGiver,
};

const HediffGiver_BrainInjury = {
  letterLabel: true,
  letter: true,
};
const HediffGiver = {
  ...HediffGiver_BrainInjury,
};

const HediffStage = {
  label: true,
};

const InjuryProps = {
  destroyedLabel: true,
  destroyedOutLabel: true,
};

// PawnKindDef

const PawnKindLifeStage = {
  label: true,
  labelPlural: true,
  labelMale: true,
  labelMalePlural: true,
  labelFemale: true,
  labelFemalePlural: true,
};

// ThingDef

const CompProperties_Schedule = {
  offMessage: true,
};
const CompProperties_Usable = {
  useLabel: true,
};

const IngestibleProperties = {
  ingestCommandString: true,
  ingestReportString: true,
  ingestReportStringEat: true,
};

const RaceProperties = {
  meatLabel: true,
};

const StuffProperties = {
  stuffAdjective: true,
};

// RulePackDef TaleDef

const Rule = {
  keyword: true,
};
const Rule_File = {
  ...Rule,
  path: true,
  pathList: SchemaType.TranslationCanChangeCount,
};
const Rule_NamePerson = {
  ...Rule,
};
const Rule_String = {
  ...Rule,
  output: true,
};

const RulePack = {
  rulesStrings: SchemaType.TranslationCanChangeCount,
  rulesFiles: SchemaType.TranslationCanChangeCount,
};

// ScenarioDef

const Scenario = {
  name: true,
  summary: true,
  description: true,
};

// StatDef RoomStatDef

const StatPart_RoomStat = {
  customLabel: true,
};

const RoomStatScoreStage = {
  label: true,
};

// ThoughtDef

const ThoughtStage = {
  label: true,
  labelSocial: true,
  description: true,
};

// TraitDef

const TraitDegreeData = {
  label: true,
  description: true,
};

// label customLabel

/**
 * Default schema for resolve definitions and extracting injections.
 */
export const schema = {
  Def: {
    label: true,
    description: true,
  },
  ApparelLayerDef: SchemaType.Def,
  BillRepeatModeDef: SchemaType.Def,
  BillStoreModeDef: SchemaType.Def,
  BiomeDef: SchemaType.Def,
  BodyDef: SchemaType.Special,
  BodyPartDef: {
    labelShort: true,
  },
  BodyPartGroupDef: {
    labelShort: true,
  },
  BodyPartTagDef: SchemaType.NoTranslate,
  BodyTypeDef: SchemaType.NoTranslate,
  ChemicalDef: SchemaType.Def,
  ClamorDef: SchemaType.NoTranslate,
  ConceptDef: {
    helpText: true,
  },
  DamageArmorCategoryDef: SchemaType.NoTranslate,
  DamageDef: {
    deathMessage: '{0} has been killed.',
  },
  DesignationCategoryDef: SchemaType.Def,
  DesignationDef: SchemaType.NoTranslate,
  DesignatorDropdownGroupDef: SchemaType.NoTranslate,
  DifficultyDef: SchemaType.Def,
  DutyDef: SchemaType.NoTranslate,
  EffecterDef: SchemaType.NoTranslate,
  FactionDef: {
    fixedName: true,
    pawnSingular: 'member',
    pawnsPlural: 'members',
    leaderTitle: 'leader',
    apparelStuffFilter: {
      ...ThingFilter,
    },
  },
  FeatureDef: SchemaType.NoTranslate,
  FleshTypeDef: SchemaType.NoTranslate,
  GameConditionDef: {
    endMessage: true,
  },
  GenStepDef: SchemaType.NoTranslate,
  HairDef: SchemaType.Def,
  HediffDef: {
    labelNoun: true,
    comps: {
      li: {
        ...HediffCompProperties,
      },
    },
    hediffGivers: {
      li: {
        ...HediffGiver,
      },
    },
    injuryProps: {
      ...InjuryProps,
    },
    stages: {
      li: {
        ...HediffStage,
      },
    },
  },
  HediffGiverSetDef: {
    hediffGivers: {
      li: {
        ...HediffGiver,
      },
    },
  },
  HibernatableStateDef: SchemaType.NoTranslate,
  HistoryAutoRecorderDef: {
    graphLabelY: true,
  },
  HistoryAutoRecorderGroupDef: SchemaType.Def,
  ImpactSoundTypeDef: SchemaType.NoTranslate,
  ImplementOwnerTypeDef: SchemaType.NoTranslate,
  IncidentCategoryDef: SchemaType.NoTranslate,
  IncidentDef: {
    letterText: true,
    letterLabel: true,
  },
  IncidentTargetTypeDef: SchemaType.NoTranslate,
  InspirationDef: {
    beginLetter: true,
    beginLetterLabel: true,
    endMessage: true,
    baseInspectLine: true,
  },
  InstructionDef: {
    text: true,
    rejectInputMessage: true,
    onMapInstruction: true,
  },
  InteractionDef: {
    logRulesInitiator: {
      ...RulePack,
    },
    logRulesRecipient: {
      ...RulePack,
    },
  },
  JobDef: {
    reportString: 'Doing something.',
  },
  JoyGiverDef: SchemaType.NoTranslate,
  JoyKindDef: SchemaType.Def,
  KeyBindingCategoryDef: SchemaType.Def,
  KeyBindingDef: SchemaType.Def,
  LetterDef: SchemaType.NoTranslate,
  LifeStageDef: {
    adjective: true,
  },
  LogEntryDef: SchemaType.NoTranslate,
  MainButtonDef: SchemaType.Def,
  ManeuverDef: SchemaType.NoTranslate,
  MapGeneratorDef: SchemaType.NoTranslate,
  MentalBreakDef: SchemaType.Def,
  MentalStateDef: {
    beginLetter: true,
    beginLetterLabel: true,
    recoveryMessage: true,
    baseInspectLine: true,
  },
  MessageTypeDef: SchemaType.NoTranslate,
  NeedDef: SchemaType.Def,
  PawnCapacityDef: {
    labelMechanoids: '',
    labelAnimals: '',
  },
  PawnColumnDef: {
    headerTip: true,
  },
  PawnGroupKindDef: SchemaType.NoTranslate,
  PawnKindDef: {
    labelPlural: '',
    labelMale: true,
    labelMalePlural: true,
    labelFemale: true,
    labelFemalePlural: true,
    lifeStages: {
      li: {
        ...PawnKindLifeStage,
      },
    },
  },
  PawnRelationDef: {
    labelFemale: true,
  },
  PawnTableDef: SchemaType.NoTranslate,
  PawnsArrivalModeDef: {
    textEnemy: true,
    textFriendly: true,
  },
  PrisonerInteractionModeDef: SchemaType.Def,
  RaidStrategyDef: {
    arrivalTextFriendly: true,
    arrivalTextEnemy: true,
    letterLabelEnemy: true,
    letterLabelFriendly: true,
  },
  RecipeDef: {
    jobString: 'Doing an unknown recipe.',
    successfullyRemovedHediffMessage: true,
  },
  RecordDef: SchemaType.Def,
  ResearchProjectDef: {
    descriptionDiscovered: true,
  },
  ResearchProjectTagDef: SchemaType.NoTranslate,
  ResearchTabDef: SchemaType.Def,
  ReservationLayerDef: SchemaType.NoTranslate,
  RiverDef: SchemaType.Def,
  RoadDef: SchemaType.Def,
  RoadPathingDef: SchemaType.NoTranslate,
  RoadWorldLayerDef: SchemaType.NoTranslate,
  RoofDef: SchemaType.Def,
  RoomRoleDef: SchemaType.Def,
  RoomStatDef: {
    scoreStages: {
      li: {
        ...RoomStatScoreStage,
      },
    },
  },
  RuleDef: SchemaType.NoTranslate,
  RulePackDef: {
    rulePack: {
      ...RulePack,
    },
  },
  ScatterableDef: SchemaType.NoTranslate,
  ScenPartDef: SchemaType.Def,
  ScenarioDef: {
    scenario: {
      ...Scenario,
    },
  },
  ShaderTypeDef: SchemaType.NoTranslate,
  SiteCoreDef: {
    ...SiteDefBase,
  },
  SitePartDef: {
    ...SiteDefBase,
    descriptionDialogue: true,
  },
  SkillDef: {
    skillLabel: true,
  },
  SongDef: SchemaType.NoTranslate,
  SoundDef: SchemaType.NoTranslate,
  SpecialThingFilterDef: SchemaType.Def,
  StatCategoryDef: SchemaType.Def,
  StatDef: {
    formatString: true,
    parts: {
      li: {
        customLabel: true,
      },
    },
  },
  StoryEventDef: SchemaType.NoTranslate,
  StorytellerDef: SchemaType.Def,
  StuffAppearanceDef: SchemaType.NoTranslate,
  StuffCategoryDef: SchemaType.Def,
  SubcameraDef: SchemaType.NoTranslate,
  TaleDef: {
    rulePack: {
      ...RulePack,
    },
  },
  TerrainAffordanceDef: SchemaType.Def,
  TerrainDef: SchemaType.Def,
  ThingCategoryDef: SchemaType.Def,
  ThingDef: {
    comps: {
      li: {
        ...CompProperties_Schedule,
        ...CompProperties_Usable,
      },
    },
    ingestible: {
      ...IngestibleProperties,
    },
    race: {
      ...RaceProperties,
    },
    stuffProps: {
      ...StuffProperties,
    },
    tools: {
      li: {
        ...Tool,
      },
    },
    verbs: {
      li: {
        ...VerbProperties,
      },
    },
  },
  ThingSetMakerDef: SchemaType.NoTranslate,
  ThinkTreeDef: SchemaType.NoTranslate,
  ThoughtDef: {
    stages: {
      li: {
        ...ThoughtStage,
      },
    },
  },
  TimeAssignmentDef: SchemaType.Def,
  ToolCapacityDef: SchemaType.NoTranslate,
  TraderKindDef: SchemaType.Def,
  TrainabilityDef: SchemaType.Def,
  TrainableDef: SchemaType.Def,
  TraitDef: {
    degreeDatas: {
      li: {
        ...TraitDegreeData,
      },
    },
  },
  TransferableSorterDef: SchemaType.Def,
  WeatherDef: SchemaType.Def,
  WorkGiverDef: {
    verb: true,
    gerund: true,
  },
  WorkGiverEquivalenceGroupDef: SchemaType.NoTranslate,
  WorkTypeDef: {
    labelShort: true,
    pawnLabel: true,
    gerundLabel: true,
    verb: true,
  },
  WorldGenStepDef: SchemaType.NoTranslate,
  WorldObjectDef: SchemaType.Def,
};
