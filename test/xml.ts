/**
 * Test definition.ts
 */

import fs from 'fs';
import globby from 'globby';
import init from '../scripts/env-init';
import { RawContents } from '../core/utils';
import * as xml from '../core/xml';

const { dirCore } = init();

async function readDefFiles(): Promise<RawContents> {
  const defFiles: string[] = await globby(`${dirCore}/Defs/**/*.xml`);
  // const defFiles: string[] = ['temp.xml'];

  return new Promise<RawContents>((resolve, reject) => {
    const rawContents: RawContents = {};
    let count: number = 0;

    defFiles.forEach(path =>
      fs.readFile(path, { encoding: 'utf-8' }, (error, content) => {
        if (error) {
          reject(error);
        }
        rawContents[path] = content;
        count++;
        if (count === defFiles.length) {
          resolve(rawContents);
        }
      }),
    );
  });
}

async function test(): Promise<void> {
  const rawContents: RawContents = await readDefFiles();

  let count: number = 0;
  Object.entries(rawContents).forEach(([path, content]) => {
    console.log(path);
    const root: xml.Element = xml.parse(content);
    count++;
  });
  console.log(count);
}

test().catch(error => console.log(error));
