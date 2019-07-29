import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import { pathsTypePackage } from './utils.test';
import { ATTRIBUTE_MUST_TRANSLATE } from './constants';
import { ExtractorEventEmitter } from './extractor-event-emitter';
import {
  ClassInfo,
  FieldInfo,
  TypePackage,
  TypePackageExtractor,
  TypeMaps,
} from './type-package';

describe('type-package', () => {
  const emitter = new ExtractorEventEmitter();
  const extractor = new TypePackageExtractor(emitter);

  let maps: TypeMaps;

  beforeAll(async () => {
    maps = await extractor.load([...pathsTypePackage, io.join(__dirname, 'Mock')]);
  });

  test('load', async () => {
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
});
