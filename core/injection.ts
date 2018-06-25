/**
 * Injection
 */

import * as logger from './logger';
import * as xml from './xml';
import { DefinitionData } from './definition';
import { RawContents, stringCompare } from './utils';
import { schema } from './schema';

export interface Field {
  name: string;
  value: string;
  children: Field[];
}

export interface Injection {
  defType: string;
  defName: string;
  commentBefore?: string;
  fields: Field[];
}

export interface InjectionData {
  [defType: string]: Injection[];
}

/**
 * Parse the definition data and extract injection data.
 */
export function parse(rawContents: RawContents): void {
  //
}
