import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import { ClassInfo, FieldInfo, ATTRIBUTE_MUST_TRANSLATE, load } from './type-package';

const resolvePath = genPathResolve(__dirname, '..', '..');
const paths = [
  resolvePath('Reflection', 'type-package.json'),
  resolvePath('Reflection', 'type-package-fix.json'),
  resolvePath('Mock', 'Assemblies'),
];

describe('type-package', () => {
  let map: Record<string, ClassInfo>;
  beforeAll(async () => {
    map = await load(paths);
  });

  test('load', async () => {
    expect(map.Def).toBeTruthy();
    expect(map.Def.name).toBe('Def');

    const classes = Object.values(map);

    classes
      .filter(ci => ci.baseClass === 'Def')
      .forEach(ci => {
        expect(ci.fields.some(f => f.name === 'label'));
        expect(ci.fields.some(f => f.name === 'description'));
      });

    classes.forEach(ci => expect(ci.handles.length).toBeGreaterThanOrEqual(0));

    // detect ThingDef inherited all fields of BuildableDef
    map.BuildableDef.fields.forEach(f => {
      expect(map.ThingDef.fields.includes(f)).toBe(true);
    });

    map.FactionDef.fields
      .filter(fi => fi.name === 'leaderTitle')
      .forEach(fieldInfo =>
        expect(fieldInfo.attributes.includes(ATTRIBUTE_MUST_TRANSLATE)).toBe(true),
      );
  });
});
