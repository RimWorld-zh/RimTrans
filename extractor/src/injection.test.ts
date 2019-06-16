import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import * as xml from './xml';
import * as definition from './definition';
import * as typePackage from './type-package';
import { parse } from './injection';

const resolvePath = genPathResolve(__dirname, '..', '..');

const pathDefs = resolvePath('Core', 'Defs');
const pathTypePackage = resolvePath('Reflection', 'type-info.json');

describe('injection', () => {
  let defMaps: definition.DefDocumentMap[];
  let classInfoMap: Record<string, typePackage.ClassInfo>;

  beforeAll(async () => {
    [defMaps, classInfoMap] = await Promise.all([
      definition.load([pathDefs]).then(definition.resolveInheritance),
      typePackage.load([pathTypePackage]),
    ]);
    defMaps.push({
      'mock.xml': xml.parse(
        `<Defs>
          
          <MockDef>
            <defName>Mock</defName>
            <label>Mock</label>
            <some>
              <li>
                <li>
                  <a>a</a>
                  <b>b</b>
                </li>
              </li>
            </some>
          </MockDef>
          
          <UnknownDef>
            <defName>Unknown</defName>
            <label>Unknown</label>
          </UnknownDef>

        </Defs>`,
      ),
    });
    classInfoMap.MockDef = {
      isAbstract: false,
      baseClass: 'Def',
      name: 'MockDef',
      fields: [
        ...classInfoMap.Def.fields,
        {
          attributes: [],
          name: 'some',
          type: {
            category: 'LIST',
            name: 'List',
            of: {
              category: 'LIST',
              name: 'List',
              of: {
                category: 'CLASS',
                name: 'SomeObject',
                of: null,
              },
            },
          },
        },
      ],
    };
    classInfoMap.SomeObject = {
      isAbstract: false,
      baseClass: '',
      name: 'SomeObject',
      fields: [
        {
          name: 'a',
          attributes: [typePackage.ATTRIBUTE_MUST_TRANSLATE],
          type: {
            category: 'VALUE',
            name: 'String',
            of: null,
          },
        },
        {
          name: 'b',
          attributes: [],
          type: {
            category: 'VALUE',
            name: 'String',
            of: null,
          },
        },
      ],
    };
  });

  test('parse', async () => {
    const injectionMap = await parse(defMaps, classInfoMap);
    await io.save(
      resolvePath('.tmp', 'TestLanguage.json'),
      JSON.stringify(injectionMap, undefined, '  '),
    );
  });
});
