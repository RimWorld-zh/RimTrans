// tslint:disable:typedef variable-name

export enum DefSchemaType {
  NoTranslate,
  Def,
}

export enum FieldSchemaType {
  TranslationCanChangeCount,
  MayTranslate,
  MustTranslate,

  // MustExtract
  SameToLabel,
}

export interface SchemaDefinition {
  [field: string]: boolean | string | SchemaDefinition | FieldSchemaType;
}

export interface Schema {
  [defType: string]: SchemaDefinition | DefSchemaType | undefined;
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

// HACK: for BodyDef, recursively
const BodyPartRecord = {
  customLabel: true,
  parts: {
    li: {},
  },
};

BodyPartRecord.parts.li = BodyPartRecord;

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
  pathList: FieldSchemaType.TranslationCanChangeCount,
};
const Rule_NamePerson = {
  ...Rule,
};
const Rule_String = {
  ...Rule,
  output: true,
};

const RulePack = {
  rulesStrings: FieldSchemaType.TranslationCanChangeCount,
  rulesFiles: FieldSchemaType.TranslationCanChangeCount,
};

// ScenarioDef

const ScenPart_GameStartDialog = {
  text: true,
};

const ScenPart = {
  ...ScenPart_GameStartDialog,
};

const Scenario = {
  name: true,
  summary: true,
  description: true,
  parts: {
    li: {
      ...ScenPart,
    },
  },
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

/**
 * Default schema for resolve definitions and extracting injections.
 */
export const schema = {
  Def: {
    label: true,
    description: true,
  },
  ApparelLayerDef: DefSchemaType.Def,
  BillRepeatModeDef: DefSchemaType.Def,
  BillStoreModeDef: DefSchemaType.Def,
  BiomeDef: DefSchemaType.Def,
  BodyDef: {
    corePart: BodyPartRecord,
  },
  BodyPartDef: {
    labelShort: true,
  },
  BodyPartGroupDef: {
    labelShort: true,
  },
  BodyPartTagDef: DefSchemaType.NoTranslate,
  BodyTypeDef: DefSchemaType.NoTranslate,
  ChemicalDef: DefSchemaType.Def,
  ClamorDef: DefSchemaType.NoTranslate,
  ConceptDef: {
    helpText: true,
  },
  DamageArmorCategoryDef: DefSchemaType.NoTranslate,
  DamageDef: {
    deathMessage: '{0} has been killed.',
  },
  DesignationCategoryDef: DefSchemaType.Def,
  DesignationDef: DefSchemaType.NoTranslate,
  DesignatorDropdownGroupDef: DefSchemaType.NoTranslate,
  DifficultyDef: DefSchemaType.Def,
  DutyDef: DefSchemaType.NoTranslate,
  EffecterDef: DefSchemaType.NoTranslate,
  FactionDef: {
    fixedName: true,
    pawnSingular: 'member',
    pawnsPlural: 'members',
    leaderTitle: 'leader',
    apparelStuffFilter: {
      ...ThingFilter,
    },
  },
  FeatureDef: DefSchemaType.NoTranslate,
  FleshTypeDef: DefSchemaType.NoTranslate,
  GameConditionDef: {
    endMessage: true,
  },
  GenStepDef: DefSchemaType.NoTranslate,
  HairDef: DefSchemaType.Def,
  HediffDef: {
    labelNoun: true,
    battleStateLabel: true,
    labelNounPretty: true,
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
  HibernatableStateDef: DefSchemaType.NoTranslate,
  HistoryAutoRecorderDef: {
    graphLabelY: true,
  },
  HistoryAutoRecorderGroupDef: DefSchemaType.Def,
  ImpactSoundTypeDef: DefSchemaType.NoTranslate,
  ImplementOwnerTypeDef: DefSchemaType.NoTranslate,
  IncidentCategoryDef: DefSchemaType.NoTranslate,
  IncidentDef: {
    letterText: true,
    letterLabel: true,
  },
  IncidentTargetTypeDef: DefSchemaType.NoTranslate,
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
  JoyGiverDef: DefSchemaType.NoTranslate,
  JoyKindDef: DefSchemaType.Def,
  KeyBindingCategoryDef: DefSchemaType.Def,
  KeyBindingDef: DefSchemaType.Def,
  LetterDef: DefSchemaType.NoTranslate,
  LifeStageDef: {
    adjective: true,
  },
  LogEntryDef: DefSchemaType.NoTranslate,
  MainButtonDef: DefSchemaType.Def,
  ManeuverDef: DefSchemaType.NoTranslate,
  MapGeneratorDef: DefSchemaType.NoTranslate,
  MentalBreakDef: DefSchemaType.Def,
  MentalStateDef: {
    beginLetter: true,
    beginLetterLabel: true,
    recoveryMessage: true,
    baseInspectLine: true,
  },
  MessageTypeDef: DefSchemaType.NoTranslate,
  NeedDef: DefSchemaType.Def,
  PawnCapacityDef: {
    labelMechanoids: true,
    labelAnimals: true,
  },
  PawnColumnDef: {
    headerTip: true,
  },
  PawnGroupKindDef: DefSchemaType.NoTranslate,
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
    labelFemale: FieldSchemaType.SameToLabel,
  },
  PawnTableDef: DefSchemaType.NoTranslate,
  PawnsArrivalModeDef: {
    textEnemy: true,
    textFriendly: true,
  },
  PrisonerInteractionModeDef: DefSchemaType.Def,
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
  RecordDef: DefSchemaType.Def,
  ResearchProjectDef: {
    descriptionDiscovered: true,
  },
  ResearchProjectTagDef: DefSchemaType.NoTranslate,
  ResearchTabDef: DefSchemaType.Def,
  ReservationLayerDef: DefSchemaType.NoTranslate,
  RiverDef: DefSchemaType.Def,
  RoadDef: DefSchemaType.Def,
  RoadPathingDef: DefSchemaType.NoTranslate,
  RoadWorldLayerDef: DefSchemaType.NoTranslate,
  RoofDef: DefSchemaType.Def,
  RoomRoleDef: DefSchemaType.Def,
  RoomStatDef: {
    scoreStages: {
      li: {
        ...RoomStatScoreStage,
      },
    },
  },
  RuleDef: DefSchemaType.NoTranslate,
  RulePackDef: {
    rulePack: {
      ...RulePack,
    },
  },
  ScatterableDef: DefSchemaType.NoTranslate,
  ScenPartDef: DefSchemaType.Def,
  ScenarioDef: {
    scenario: {
      ...Scenario,
    },
  },
  ShaderTypeDef: DefSchemaType.NoTranslate,
  SiteCoreDef: {
    ...SiteDefBase,
  },
  SitePartDef: {
    ...SiteDefBase,
    descriptionDialogue: true,
  },
  SkillDef: {
    skillLabel: FieldSchemaType.SameToLabel,
  },
  SongDef: DefSchemaType.NoTranslate,
  SoundDef: DefSchemaType.NoTranslate,
  SpecialThingFilterDef: DefSchemaType.Def,
  StatCategoryDef: DefSchemaType.Def,
  StatDef: {
    formatString: true,
    parts: {
      li: {
        customLabel: true,
      },
    },
  },
  StoryEventDef: DefSchemaType.NoTranslate,
  StorytellerDef: DefSchemaType.Def,
  StuffAppearanceDef: DefSchemaType.NoTranslate,
  StuffCategoryDef: DefSchemaType.Def,
  SubcameraDef: DefSchemaType.NoTranslate,
  TaleDef: {
    rulePack: {
      ...RulePack,
    },
  },
  TerrainAffordanceDef: DefSchemaType.Def,
  TerrainDef: DefSchemaType.Def,
  ThingCategoryDef: DefSchemaType.Def,
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
  ThingSetMakerDef: DefSchemaType.NoTranslate,
  ThinkTreeDef: DefSchemaType.NoTranslate,
  ThoughtDef: {
    stages: {
      li: {
        ...ThoughtStage,
      },
    },
  },
  TimeAssignmentDef: DefSchemaType.Def,
  ToolCapacityDef: DefSchemaType.NoTranslate,
  TraderKindDef: DefSchemaType.Def,
  TrainabilityDef: DefSchemaType.Def,
  TrainableDef: DefSchemaType.Def,
  TraitDef: {
    degreeDatas: {
      li: {
        ...TraitDegreeData,
      },
    },
  },
  TransferableSorterDef: DefSchemaType.Def,
  WeatherDef: DefSchemaType.Def,
  WorkGiverDef: {
    verb: true,
    gerund: true,
  },
  WorkGiverEquivalenceGroupDef: DefSchemaType.NoTranslate,
  WorkTypeDef: {
    labelShort: true,
    pawnLabel: true,
    gerundLabel: true,
    verb: true,
  },
  WorldGenStepDef: DefSchemaType.NoTranslate,
  WorldObjectDef: DefSchemaType.Def,
};
