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
  definition.postProcess(defDataList);

  const injData: Dictionary<injection.Injection[]> = injection.extract(defData);
  const languageData: Dictionary<string> = injection.generateXMLContents(injData);

  // Object.entries(injData).forEach(([defType, injs]) => {
  //   console.log();
  //   console.log(chalk.greenBright(defType));
  //   // console.log(injs.map(inj => inj.defName).join(', '));
  //   injs.forEach(inj => {
  //     fs.writeFile(
  //       `temp/${defType}--${inj.defName}.json`,
  //       JSON.stringify(inj, undefined, '  '),
  //       error => {
  //         if (error) {
  //           console.error(error);
  //         }
  //       },
  //     );
  //   });
  // });

  if (fs.existsSync('temp')) {
    rm.sync('temp');
  }
  fs.mkdirSync('temp');
  Object.entries(languageData).forEach(([p, content]) => {
    const file: string = path.join('temp', p);
    const dir: string = path.dirname(file);
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir);
    }
    fs.writeFile(file, content, error => {
      if (error) {
        console.log(error);
      }
    });
  });
}

test().catch(error => console.error(error));
