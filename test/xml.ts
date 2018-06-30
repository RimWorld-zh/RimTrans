/**
 * Test definition.ts
 */

import fs from 'fs';
import globby from 'globby';
import { readRawContents } from '../scripts/utils';
import init from '../scripts/env-init';
import * as xml from '../core/xml';
import { Dictionary } from '../common/collection';

const { dirCore } = init();

async function test(): Promise<void> {
  const rawContents: Dictionary<string> = await readRawContents(
    `${dirCore}/Defs/**/*.xml`,
  );

  let count: number = 0;
  Object.entries(rawContents).forEach(([path, content]) => {
    console.log(path);
    const root: xml.Element = xml.parse(content);
    count++;
  });
  console.log(count);
}

test().catch(error => console.log(error));

const element: xml.Element = xml.parse('<test>test</test>');

console.log(JSON.stringify(element, undefined, '  '));
