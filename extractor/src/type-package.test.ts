import pth from 'path';
import fse from 'fs-extra';
import globby from 'globby';

import { ATTRIBUTE_MUST_TRANSLATE } from './constants';

import { ExtractorEventEmitter } from './extractor-event-emitter';
import {
  ClassInfo,
  FieldInfo,
  TypePackage,
  TypeMaps,
  TypePackageExtractor,
  getCoreTypePackage,
} from './type-package';

const emitter = new ExtractorEventEmitter();
const extractor = new TypePackageExtractor(emitter);

describe('type-package', () => {
  let maps: TypeMaps;

  beforeAll(async () => {
    maps = extractor.merge([await getCoreTypePackage()]);
  });

  test('getCoreTypePackage', async () => {
    const core = getCoreTypePackage();
    expect(core.classes).toBeDefined();
    expect(core.enums).toBeDefined();
    expect(core.fix).toBeDefined();
  });

  test('core', async () => {
    const { classInfoMap } = maps;

    expect(classInfoMap.Def).toBeTruthy();
    expect(classInfoMap.Def.name).toBe('Def');

    const allClasses = Object.values(classInfoMap);

    allClasses
      .filter(ci => ci.baseClass === 'Def')
      .forEach(ci => {
        expect(ci.fields.some(f => f.name === 'label'));
        expect(ci.fields.some(f => f.name === 'description'));
      });

    allClasses.forEach(ci => expect(ci.handles.length).toBeGreaterThanOrEqual(0));

    // detect ThingDef inherited all fields of BuildableDef
    classInfoMap.BuildableDef.fields.forEach(f => {
      expect(classInfoMap.ThingDef.fields.includes(f)).toBe(true);
    });

    classInfoMap.FactionDef.fields
      .filter(fi => fi.name === 'leaderTitle')
      .forEach(fieldInfo =>
        expect(fieldInfo.attributes.includes(ATTRIBUTE_MUST_TRANSLATE)).toBe(true),
      );
  });

  test('load', async () => {
    const pkg = await extractor.load(pth.join(__dirname, 'Mock'));
    expect(pkg).toEqual({});
  });
});
