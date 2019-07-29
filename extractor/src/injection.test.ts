import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import {
  pathsDefs,
  pathsDefInjected,
  pathsTypePackage,
  outputInjectionMapLoaded,
  outputInjectionMapParsed,
  outputInjectionMapParsedFuzzy,
  outputInjectionMapMerged,
  outputMissing,
  outputFuzzy,
  outputDefInjected,
  outputDefInjectedFuzzy,
} from './utils.test';
import {
  ATTRIBUTE_MUST_TRANSLATE,
  ATTRIBUTE_TRANSLATION_CAN_CHANGE_COUNT,
} from './constants';
import { ExtractorEventEmitter } from './extractor-event-emitter';
import { parseXML } from './xml';
import { ClassInfo, TypePackage, TypePackageExtractor } from './type-package';
import { DefsElementMap, DefinitionExtractor } from './definition';
import { PathNode, Injection, InjectionMap, InjectionExtractor } from './injection';

describe('injection', () => {
  const emitter = new ExtractorEventEmitter();
  const typePackageExtractor = new TypePackageExtractor(emitter);
  const definitionExtractor = new DefinitionExtractor(emitter);
  const injectionExtractor = new InjectionExtractor(emitter);

  let defMaps: DefsElementMap[];
  let classInfoMap: Record<string, ClassInfo>;
  let injectionMapsLoaded: InjectionMap[];
  let injectionMapsParsed: InjectionMap[];
  let injectionMapsParsedFuzzy: InjectionMap[];

  beforeAll(async () => {
    [defMaps, { classInfoMap }] = await Promise.all([
      definitionExtractor
        .load(pathsDefs)
        .then(maps => definitionExtractor.resolveInheritance(maps)),
      typePackageExtractor.load(pathsTypePackage),
    ]);
    defMaps[0]['zmocks_1.xml'] = parseXML(
      `<Defs>
        <ZMockDef>
          <defName>Mock1</defName>
          <label>label</label>
          <description>description</description>
          <some>
            <li>mock text</li>
            <li>mock text</li>
          </some>
          <someList>
            <li>
              <li>
                <a>a0</a>
              </li>
              <li>
                <a>a1</a>
              </li>
            </li>
          </someList>
          <fuzzy>mock text</fuzzy>
          <missing>
            <key>x</key>
            <value>0</value>
          </missing>
        </ZMockDef>
      </Defs>`,
    );
    defMaps.push({
      'mock.xml': parseXML(
        `<Defs>          
          <UnknownDef>
            <defName>Unknown</defName>
            <label>Unknown</label>
          </UnknownDef>
        </Defs>`,
      ),
    });
    classInfoMap.ZMockDef = {
      isAbstract: false,
      baseClass: 'Def',
      name: 'ZMockDef',
      fields: [
        ...classInfoMap.Def.fields,
        {
          name: 'some',
          attributes: [ATTRIBUTE_MUST_TRANSLATE, ATTRIBUTE_TRANSLATION_CAN_CHANGE_COUNT],
          type: {
            category: 'LIST',
            name: 'List',
            of: {
              category: 'VALUE',
              name: 'String',
            },
          },
        },
        {
          name: 'someList',
          attributes: [],
          type: {
            category: 'LIST',
            name: 'List',
            of: {
              category: 'LIST',
              name: 'List',
              of: {
                category: 'CLASS',
                name: 'ZMockClass',
              },
            },
          },
        },
        {
          name: 'fuzzy',
          attributes: [],
          type: {
            category: 'VALUE',
            name: 'String',
          },
        },
        {
          name: 'missing',
          attributes: [],
          type: {
            category: 'CLASS',
            name: 'ZMockMissingClass',
          },
        },
      ],
      handles: [],
    };
    classInfoMap.ZMockClass = {
      name: 'ZMockClass',
      isAbstract: false,
      fields: [
        {
          attributes: [],
          name: 'a',
          type: {
            category: 'VALUE',
            name: 'String',
          },
        },
      ],
      handles: [],
    };

    [
      injectionMapsLoaded,
      injectionMapsParsed,
      injectionMapsParsedFuzzy,
    ] = await Promise.all([
      injectionExtractor.load(pathsDefInjected),
      injectionExtractor.parse(defMaps, classInfoMap),
      injectionExtractor.parse(defMaps, classInfoMap, true),
    ]);
  });

  test('path', () => {
    expect(injectionExtractor.pathMatch(['Mock', 'label'], ['Mock', 'label'])).toBe(true);
    expect(injectionExtractor.pathMatch(['Mock', 'label'], ['Mock', 'description'])).toBe(
      false,
    );
    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'comps', 'compUsable', 'label'],
        ['Mock', 'comps', 'compUsable'],
      ),
    ).toBe(false);
    expect(injectionExtractor.pathMatch(['Mock', 'comps', 0], ['Mock', 'comps', 1])).toBe(
      false,
    );

    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(true);
    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', [1, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(false);
    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', [0, 'compUsable-0'], 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable-1'], 'a', 'label'],
      ),
    ).toBe(false);

    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', 0, 'a', 'label'],
      ),
    ).toBe(true);
    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', 1, 'a', 'label'],
      ),
    ).toBe(false);

    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', 'compUsable', 'a', 'label'],
      ),
    ).toBe(true);
    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
        ['Mock', 'some', 'compUsable-1', 'a', 'label'],
      ),
    ).toBe(false);

    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', 0, 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(true);
    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', 1, 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(false);

    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', 'compUsable', 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(true);
    expect(
      injectionExtractor.pathMatch(
        ['Mock', 'some', 'compUsable-1', 'a', 'label'],
        ['Mock', 'some', [0, 'compUsable'], 'a', 'label'],
      ),
    ).toBe(false);

    expect(injectionExtractor.serializePath(['Mock', 'some', 'a', 'label'])).toBe(
      'Mock.some.a.label',
    );
    expect(injectionExtractor.serializePath(['Mock', 'some', 0, 'label'])).toBe(
      'Mock.some.0.label',
    );
    expect(
      injectionExtractor.serializePath(['Mock', 'some', [0, 'compUsable'], 'a', 'label']),
    ).toBe('Mock.some.compUsable.a.label');

    expect(injectionExtractor.deSerializePath('Mock.some.0.label')).toEqual([
      'Mock',
      'some',
      0,
      'label',
    ]);
    expect(injectionExtractor.deSerializePath('Mock.some.1.label')).toEqual([
      'Mock',
      'some',
      1,
      'label',
    ]);
  });

  test('load', async () => {
    expect(
      injectionMapsLoaded[0].ZMockDef.zmocks_1.some(
        inj =>
          typeof inj === 'object' &&
          injectionExtractor.pathMatch(inj.path, ['Mock1', 'label']) &&
          inj.translation === 'mock label',
      ),
    ).toBe(true);

    await io.save(
      outputInjectionMapLoaded,
      JSON.stringify(injectionMapsLoaded, undefined, '  '),
    );
  });

  test('parse', async () => {
    expect(
      injectionMapsParsed[0].BiomeDef.Biomes_Temperate.some(
        inj =>
          typeof inj === 'object' &&
          injectionExtractor.pathMatch(inj.path, ['TemperateForest', 'label']) &&
          inj.origin === 'temperate forest' &&
          inj.translation === 'TODO',
      ),
    );

    await Promise.all([
      io.save(
        outputInjectionMapParsed,
        JSON.stringify(injectionMapsParsed, undefined, '  '),
      ),
      io.save(
        outputInjectionMapParsedFuzzy,
        JSON.stringify(injectionMapsParsedFuzzy, undefined, '  '),
      ),
    ]);
  });

  test('missing', async () => {
    const missing: string[] = [];
    const [mapOld] = injectionMapsLoaded;
    const [mapNew] = injectionMapsParsed;

    expect(mapNew.ZZMockDef).toBeFalsy();
    expect(mapOld.ZZMockDef).toBeTruthy();
    expect(mapOld.ZZMockDef.zmocks_1).toBeTruthy();

    Object.entries(mapOld).forEach(([defType, subMapOld]) => {
      const subMapNew = mapNew[defType] || {};
      Object.entries(subMapOld).forEach(([fileName, injectionListOld]) => {
        const injectionListNew = subMapNew[fileName] || [];
        injectionListOld.forEach(injOld => {
          if (typeof injOld === 'string') {
            expect(injectionListNew.includes(injOld));
          } else {
            const injNew = injectionListNew.find(
              inj =>
                typeof inj !== 'string' &&
                injectionExtractor.pathMatch(inj.path, injOld.path),
            );

            if ((typeof injNew === 'object' && injNew.fuzzy) || !injNew) {
              missing.push(`${defType}.${injOld.path.join('.')}`);
            }
          }
        });
      });
    });
    await io.save(outputMissing, missing.sort().join('\n'));
  });

  test('fuzzy', async () => {
    const [map] = injectionMapsParsedFuzzy;
    const fuzzy: string[] = [];
    Object.entries(map).forEach(([defType, subMap]) =>
      Object.entries(subMap).forEach(([fileName, injections]) =>
        injections.forEach(injection => {
          if (typeof injection === 'object' && injection.fuzzy) {
            fuzzy.push(`${defType}.${injection.path.slice(1).join('.')}`);
          }
        }),
      ),
    );
    await io.save(outputFuzzy, [...new Set(fuzzy)].sort().join('\n'));
  });

  test('save', async () => {
    const [mapOld] = injectionMapsLoaded;
    const [mapNew] = injectionMapsParsedFuzzy;

    const mapMerged = injectionExtractor.merge(mapNew, mapOld);
    injectionExtractor.checkDuplicated([mapMerged]);

    expect(mapMerged).not.toBe(mapOld);
    expect(mapMerged).not.toBe(mapNew);

    expect(mapNew.ZZMockDef).toBeFalsy();
    expect(mapOld.ZZMockDef).toBeTruthy();
    expect(mapOld.ZZMockDef.zmocks_1).toBeTruthy();

    await Promise.all([
      io.deleteFileOrDirectory(outputDefInjected),
      io.deleteFileOrDirectory(outputDefInjectedFuzzy),
    ]);
    await Promise.all([
      io.save(outputInjectionMapMerged, JSON.stringify(mapMerged, undefined, '  ')),
      injectionExtractor.save(outputDefInjected, mapMerged),
      injectionExtractor.save(outputDefInjectedFuzzy, mapMerged),
    ]);
  });
});
