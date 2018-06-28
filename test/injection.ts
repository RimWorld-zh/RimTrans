/**
 * Test core/injection.ts
 */

import fs from 'fs';
import chalk from 'chalk';
import { readRawContents } from '../scripts/utils';
import init from '../scripts/env-init';
import { RawContents } from '../core/utils';
import * as xml from '../core/xml';
import * as definition from '../core/definition';
import * as injection from '../core/injection';

const { dirCore } = init();

async function test(): Promise<void> {
  const rawContents: RawContents = await readRawContents(`${dirCore}/Defs/**/*.xml`);

  const defData: definition.DefinitionData = definition.parse(rawContents);

  const dataList: definition.DefinitionData[] = [defData];

  definition.resolveInheritance(dataList);
  definition.postProcess(dataList);

  const injData: injection.InjectionData = injection.extract(defData);

  Object.entries(injData).forEach(([defType, injs]) => {
    console.log();
    console.log(chalk.greenBright(defType));
    // console.log(injs.map(inj => inj.defName).join(', '));
    injs.forEach(inj => {
      fs.writeFile(
        `temp/${defType}--${inj.defName}.json`,
        JSON.stringify(inj, undefined, '  '),
        error => {
          if (error) {
            console.error(error);
          }
        },
      );
    });
  });
}

test().catch(error => console.error(error));
