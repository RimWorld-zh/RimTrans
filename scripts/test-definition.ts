/**
 * Test definition.
 */
import envInit from './env-init';
import readFiles from 'read-files';

import * as xml from '../core/xml';
import * as definition from '../core/definition';

const { dirCore } = envInit();

readFiles(`${dirCore}/Defs/**/*.xml`)
  .then(rawContents => {
    console.log(Object.entries(rawContents).length);
    const data: definition.DefinitionData = definition.parse(rawContents);
    definition.resolveInheritance([data]);
    console.log(
      JSON.stringify(
        data.TerrainDef.find(def => {
          const defName: xml.Element | undefined = def.nodes.find(
            xml.isElementByName('defName'),
          );

          return !!defName && xml.getText(defName) === 'BurnedWoodPlankFloor';
        }),
        undefined,
        '  ',
      ),
    );
  })
  .catch(error => console.error(error));
