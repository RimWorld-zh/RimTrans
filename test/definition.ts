/**
 * Test core/definition.ts
 */

import chalk from 'chalk';
import { readRawContents } from '../scripts/utils';
import init from '../scripts/env-init';
import { Dictionary } from '../common/collection';
import * as xml from '../core/xml';
import * as definition from '../core/definition';

const { dirCore } = init();

async function test(): Promise<void> {
  const rawContents: Dictionary<string> = await readRawContents(
    `${dirCore}/Defs/**/*.xml`,
  );

  const dataOrigin: Dictionary<xml.Element[]> = {};
  Object.entries(rawContents).forEach(([path, content]) => {
    const root: xml.Element = xml.parse(content);
    root.nodes.filter(xml.isElement).forEach(def => {
      if (!dataOrigin[def.name]) {
        dataOrigin[def.name] = [];
      }
      dataOrigin[def.name].push(def);
    });
  });

  const data: Dictionary<xml.Element[]> = definition.parse(rawContents);

  console.log(
    'DefType Count:',
    `Origin: ${Object.keys(dataOrigin).length};`,
    `Parsed: ${Object.keys(data).length};`,
  );

  Object.entries(data).forEach(([defType, defs]) => {
    if (defs.length !== dataOrigin[defType].length) {
      throw new Error(`Defs count not equal: ${defType}.`);
    }
  });

  const dataList: Dictionary<xml.Element[]>[] = [data];

  definition.resolveInheritance(dataList);
  definition.postProcess(dataList);

  Object.entries(data).forEach(([defType, defs]) => {
    console.log();
    console.log(chalk.greenBright(defType));
    console.log(defs.map(definition.getDefName).join(', '));
  });
}

test().catch(error => console.log(error));
