/**
 * Test definition.
 */

import fs from 'fs';
import globby from 'globby';
import * as xml from '../core/xml';
import * as definition from '../core/definition';

const PATH_TO_DEFS: string = '/mnt/f/rw/RimWorld-Core/Core/Defs';

function readFiles(): Promise<definition.RawContents> {
  return new Promise<definition.RawContents>(async (resolve, reject) => {
    const rawContents: definition.RawContents = {};

    const files: string[] = await globby(`${PATH_TO_DEFS}/**/*.xml`);
    let count: number = 0;

    files.forEach(f =>
      fs.readFile(f, { encoding: 'utf-8' }, (error, data) => {
        if (error) {
          reject(error);
        }
        rawContents[f] = data;
        count++;
        if (count === files.length) {
          resolve(rawContents);
        }
      }),
    );
  });
}

readFiles()
  .then(rawContents => {
    console.log(Object.entries(rawContents).length);
    const data: definition.DefinitionData = definition.parse(rawContents);
    definition.resolveInheritance([data]);
    console.log(
      JSON.stringify(
        data.TerrainDef.find(def => {
          const defName: xml.Element | undefined = xml.getElement(def, 'defName');

          return !!defName && xml.getText(defName) === 'BurnedWoodPlankFloor';
        }),
        undefined,
        '  ',
      ),
    );
  })
  .catch(error => console.error(error));
