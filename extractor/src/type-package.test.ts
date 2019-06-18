import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import { load } from './type-package';

const resolvePath = genPathResolve(__dirname, '..', '..');
const paths = [
  resolvePath('Reflection', 'type-package.json'),
  resolvePath('Mock', 'Assemblies'),
];

describe('type-package', () => {
  test('load', async () => {
    const map = await load(paths);

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
  });
});
