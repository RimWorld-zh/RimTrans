import { ExtractConfig } from '@rimtrans/extractor';
import { FSWatchSlaver, FSWatchIpcTypeMap } from '../fs-watcher';

export interface TranslatorProject {
  meta: TranslatorProjectMetaData;

  extractConfig: ExtractConfig;
}

export interface TranslatorProjectMetaData {
  name: string;
  timeCreated: number;
  timeLastAccess: number;
  mods: string[];
}

export type TranslatorProjectSlaver = FSWatchSlaver<
  TranslatorProject,
  TranslatorProjectMetaData
>;

export type TranslatorProjectIpcTypeMap = FSWatchIpcTypeMap<
  TranslatorProject,
  TranslatorProjectMetaData
>;
