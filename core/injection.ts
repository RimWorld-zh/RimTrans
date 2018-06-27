/**
 * Injection
 */

import * as logger from './logger';
import * as xml from './xml';
import { DefinitionData } from './definition';
import { RawContents, stringCompare } from './utils';
import { schema } from './schema';

interface FilePositionMap {
  [defType: string]: {
    [fileName: string]: Injection;
  };
}

export interface Field {
  status: number;
  name: string;
  value: string;
  children: Field[];
}

export interface Injection {
  defType: string;
  sourceFile: string;
  fileName: string;
  defName: string;
  commentBefore?: string;
  fields: Field[];
}

export interface InjectionData {
  [defType: string]: Injection[];
}

export interface InjectionMap {
  [defType: string]: {
    [defName: string]: Injection;
  };
}

/**
 * Parse the definition data and extract injection data.
 */
export function parse(rawContents: RawContents): void {
  //
}

export function inject(
  definitionData: DefinitionData,
  injectionData: InjectionData,
): void {
  //
}

export function extract(definitionData: DefinitionData): void {
  //
}
