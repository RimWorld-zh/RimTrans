// generate by script/gen-types.ts, do not edit this file by hand.
export interface TranslationDictionary {
  readonly app: {
    readonly name: string;
    readonly description: string;
    readonly tool: {
      readonly translator: string;
      readonly modder: string;
      readonly translationWorkshop: string;
    };
  };
  readonly common: {
    readonly game: string;
    readonly mod: string;
    readonly mods: string;
    readonly steam: string;
    readonly steamWorkshop: string;
    readonly rimworld: string;
  };
  readonly modMeta: {
    readonly detail: string;
    readonly name: string;
    readonly author: string;
    readonly url: string;
    readonly description: string;
    readonly targetVersion: string;
    readonly supportedVersions: string;
  };
  readonly dialog: {
    readonly ok: string;
    readonly yes: string;
    readonly no: string;
    readonly confirm: string;
    readonly cancel: string;
  };
  readonly editor: {
    readonly undo: string;
    readonly redo: string;
    readonly cut: string;
    readonly copy: string;
    readonly paste: string;
    readonly selectAll: string;
  };
  readonly file: {
    readonly file: string;
    readonly folder: string;
    readonly directory: string;
    readonly path: string;
    readonly explore: string;
    readonly view: string;
    readonly open: string;
    readonly close: string;
    readonly closeAll: string;
    readonly add: string;
    readonly delete: string;
    readonly remove: string;
    readonly load: string;
    readonly save: string;
    readonly saveAll: string;
    readonly loading: string;
  };
  readonly settings: {
    readonly features: string;
    readonly ui: string;
    readonly about: string;
    readonly directoryRimWorld: string;
    readonly directoryWorkshop: string;
    readonly darkMode: string;
  };
}

export interface TranslationDictionaryPartial {
  readonly app?: {
    readonly name?: string;
    readonly description?: string;
    readonly tool?: {
      readonly translator?: string;
      readonly modder?: string;
      readonly translationWorkshop?: string;
    };
  };
  readonly common?: {
    readonly game?: string;
    readonly mod?: string;
    readonly mods?: string;
    readonly steam?: string;
    readonly steamWorkshop?: string;
    readonly rimworld?: string;
  };
  readonly modMeta?: {
    readonly detail?: string;
    readonly name?: string;
    readonly author?: string;
    readonly url?: string;
    readonly description?: string;
    readonly targetVersion?: string;
    readonly supportedVersions?: string;
  };
  readonly dialog?: {
    readonly ok?: string;
    readonly yes?: string;
    readonly no?: string;
    readonly confirm?: string;
    readonly cancel?: string;
  };
  readonly editor?: {
    readonly undo?: string;
    readonly redo?: string;
    readonly cut?: string;
    readonly copy?: string;
    readonly paste?: string;
    readonly selectAll?: string;
  };
  readonly file?: {
    readonly file?: string;
    readonly folder?: string;
    readonly directory?: string;
    readonly path?: string;
    readonly explore?: string;
    readonly view?: string;
    readonly open?: string;
    readonly close?: string;
    readonly closeAll?: string;
    readonly add?: string;
    readonly delete?: string;
    readonly remove?: string;
    readonly load?: string;
    readonly save?: string;
    readonly saveAll?: string;
    readonly loading?: string;
  };
  readonly settings?: {
    readonly features?: string;
    readonly ui?: string;
    readonly about?: string;
    readonly directoryRimWorld?: string;
    readonly directoryWorkshop?: string;
    readonly darkMode?: string;
  };
}
