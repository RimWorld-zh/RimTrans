import { genPathResolve } from '@huiji/shared-utils';
import * as xml from './xml';

const resolvePath = genPathResolve(__dirname, '..', '..', 'Core');

describe('xml', () => {
  test('load', async () => {
    const pathBiomesCold = resolvePath('Defs', 'BiomeDefs', 'Biomes_Cold.xml');
    const { documentElement: root } = await xml.load(pathBiomesCold);

    expect(root.tagName).toBe('Defs');

    Array.from(root.children).forEach(c => {
      expect(c.tagName).toBe('BiomeDef');
    });
  });

  test('parse', () => {
    const doc = xml.parse('<Defs><MockDef></MockDef><MockDef></MockDef></Defs>');
    const { documentElement: root } = doc;

    expect(root.tagName).toBe('Defs');

    Array.from(root.children).forEach(c => {
      expect(c.tagName).toBe('MockDef');
    });

    expect(Array.from(root.children).length).toBe(2);
  });

  test('create', () => {
    const tagName = 'Defs';
    const doc = xml.create(tagName);
    const { documentElement: root } = doc;

    expect(root.tagName).toBe(tagName);
    expect(root.outerHTML).toBe(`<${tagName}/>`);
    expect(Array.from(root.children).length).toBe(0);

    root.appendChild(doc.createElement('MockDef'));
    root.appendChild(doc.createElement('MockDef'));
    expect(Array.from(root.children).length).toBe(2);
  });
});
