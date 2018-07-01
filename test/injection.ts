/**
 * Test core/injection.ts
 */

import fs from 'fs';
import path from 'path';
import rm from 'rimraf';
import chalk from 'chalk';
import init from '../scripts/env-init';
import { readRawContents } from '../scripts/utils';
import { Dictionary } from '../common/collection';
import * as xml from '../core/xml';
import * as definition from '../core/definition';
import * as injection from '../core/injection';

const { dirCore } = init();

async function test(): Promise<void> {
  const rawContents: Dictionary<string> = await readRawContents(
    `${dirCore}/Defs/**/*.xml`,
  );
  const defData: Dictionary<xml.Element[]> = definition.parse(rawContents);
  const defDataList: Dictionary<xml.Element[]>[] = [defData];
  definition.resolveInheritance(defDataList);
  const defMapList: Dictionary<Dictionary<xml.Element>>[] = definition.postProcess(
    defDataList,
  );
  definition.prepareForExtract(defMapList);

  const injData: Dictionary<injection.Injection[]> = injection.extract(defData);
  const languageData: Dictionary<string> = injection.generateXMLContents(injData);

  return new Promise<void>((resolve, reject) => {
    if (fs.existsSync('temp')) {
      rm.sync('temp');
    }
    fs.mkdirSync('temp');

    const length: number = Object.keys(languageData).length;
    let count: number = 0;

    Object.entries(languageData).forEach(([p, content]) => {
      const file: string = path.join('temp', p);
      const dir: string = path.dirname(file);
      if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir);
      }
      fs.writeFile(file, content, error => {
        if (error) {
          reject(error);
        }
        count++;
        if (count === length) {
          resolve();
        }
      });
    });
  });
}

console.time('Extract injection');
test()
  .then(() => console.timeEnd('Extract injection'))
  .catch(error => console.error(error));
