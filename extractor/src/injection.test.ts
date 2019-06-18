import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import * as xml from './xml';
import * as definition from './definition';
import * as typePackage from './type-package';
import { InjectionMap, parse, load, pathMatch } from './injection';

const resolvePath = genPathResolve(__dirname, '..', '..');

const pathDefs = resolvePath('Core', 'Defs');
const pathDefInjected = resolvePath('Core', 'Languages', 'Template', 'DefInjected');
const pathDefInjectedMock = resolvePath('Core', 'Languages', 'Mock', 'DefInjected');
const pathTypePackage = resolvePath('Reflection', 'type-package.json');

describe('injection', () => {
  let defMaps: definition.DefDocumentMap[];
  let classInfoMap: Record<string, typePackage.ClassInfo>;
  let injectionMapsLoaded: InjectionMap[];
  let injectionMapsParsed: InjectionMap[];

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
      handles: [],
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
      handles: [],
    };

    [injectionMapsLoaded, injectionMapsParsed] = await Promise.all([
      load([pathDefInjected, pathDefInjectedMock]),
      parse(defMaps, classInfoMap),
    ]);
  });

  test('load', async () => {
    await io.save(
      resolvePath('.tmp', 'injection-maps-loaded.json'),
      JSON.stringify(injectionMapsLoaded, undefined, '  '),
    );
  });

  test('parse', async () => {
    await io.save(
      resolvePath('.tmp', 'injection-maps-parsed.json'),
      JSON.stringify(injectionMapsParsed, undefined, '  '),
    );
  });

  test('pathMatch', () => {
    expect(pathMatch(['Mock', 'label'], ['Mock', 'label'])).toBe(true);
    expect(pathMatch(['Mock', 'label'], ['Mock', 'description'])).toBe(false);
    expect(
      pathMatch(
        ['Mock', 'comps', 'compUsable', 'label'],
        ['Mock', 'comps', 'compUsable'],
      ),
    ).toBe(false);
    expect(pathMatch(['Mock', 'comps', 0], ['Mock', 'comps', 1])).toBe(false);

    expect(
      pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(true);
    expect(
      pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', [1, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(false);
    expect(
      pathMatch(
        ['Mock', 'some', [0, 'compUsable-0'], 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable-1'], 'a', 'label'],
      ),
    ).toBe(false);

    expect(
      pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', 0, 'a', 'label'],
      ),
    ).toBe(true);
    expect(
      pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', 1, 'a', 'label'],
      ),
    ).toBe(false);

    expect(
      pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', 'compUsable', 'a', 'label'],
      ),
    ).toBe(true);
    expect(
      pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', 'compUsable-1', 'a', 'label'],
      ),
    ).toBe(false);

    expect(
      pathMatch(
        ['Mock', 'some', 0, 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(true);
    expect(
      pathMatch(
        ['Mock', 'some', 1, 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(false);

    expect(
      pathMatch(
        ['Mock', 'some', 'compUsable', 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(true);
    expect(
      pathMatch(
        ['Mock', 'some', 'compUsable-1', 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(false);
  });

  test('verify', () => {
    const [mapOld] = injectionMapsLoaded;
    const [mapNew] = injectionMapsParsed;
    Object.entries(mapOld).forEach(([defType, subMapOld]) => {
      const subMapNew = mapNew[defType];
      Object.entries(subMapOld).forEach(([fileName, injectionsOld]) => {
        const injectionsNew = subMapNew[fileName];
        injectionsOld.forEach(injOld => {
          if (typeof injOld === 'string') {
            expect(injectionsNew.includes(injOld));
          } else {
            const injNew = injectionsNew.find(
              inj => typeof inj !== 'string' && pathMatch(inj.path, injOld.path),
            );
            if (!injNew) {
              console.log(injOld.path.join('.'));
            }
            // expect(injNew).toBeTruthy();
          }
        });
      });
    });
  });
});
