import fs from 'fs';
import pth from 'path';
import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import { pathsDefs, defsFileCount, outputInheritedDefs } from './utils.test';
import { parseXML, saveXML } from './xml';
import { DefsElementMap, Definition } from './definition';
import { cloneObject } from './object';

describe('def', () => {
  let defMaps: DefsElementMap[];
  beforeAll(async () => {
    defMaps = await Definition.load(pathsDefs);
  });

  test('load', async () => {
    expect(Object.keys(defMaps[0]).length).toBe(defsFileCount);
  });

  test('resolveInheritance', async () => {
    // arguments error
    expect(() => Definition.resolveInheritance([])).toThrowError(/empty array/);

    // parent not found
    Definition.resolveInheritance([
      {
        'test.xml': parseXML(`
      <Defs>
        <MockDef ParentName="MockX"></MockDef>
      </Defs>`),
      },
    ]);

    // core
    const maps = await Definition.resolveInheritance(defMaps);
    const core = maps[0];
    await io.deleteFileOrDirectory(outputInheritedDefs);
    for (const [path, root] of Object.entries(core)) {
      await saveXML(io.join(outputInheritedDefs, path), root, false);
    }
    expect(Object.keys(core).length).toBe(defsFileCount);
  });

  test('resolveInheritanceNodeRecursively & resolveXmlNodeFor', () => {
    const root = parseXML(`
    <Defs>
      <MockDef Name="Mock0">
        <Some>
          <A></A>
          <B></B>
        </Some>
      </MockDef>
      <MockDef ParentName="Mock0">
        <Some>C</Some>
      </MockDef>
    </Defs>`);
    const {
      elements: [mock0, mock1],
    } = root;

    expect(() =>
      Definition.resolveInheritanceNodeRecursively({
        root,
        def: mock1,
        resolvedDef: mock1,
        parent: {
          root,
          def: mock0,
          resolvedDef: mock0,
          children: [],
        },
        children: [],
      }),
    ).toThrowError(/cyclic/);

    expect(() =>
      Definition.resolveXmlNodeFor({
        root,
        def: mock1,
        parent: {
          root,
          def: mock0,
          children: [],
        },
        children: [],
      }),
    ).toThrowError(/not been resolved yet/);
  });

  test('recursiveNodeCopyOverwriteElements', () => {
    const root = parseXML(`
    <Defs>
      <MockDef Name="Mock0">
        <Some>
          <A></A>
          <B></B>
        </Some>
      </MockDef>
      <MockDef ParentName="Mock0">
        <Some>
          <C></C>
        </Some>
      </MockDef>
      <MockDef2>
        <Some></Some>
      </MockDef2>
      <MockDef3>X</MockDef3>
    </Defs>
    `);
    const {
      elements: [mock0, mock1, mock2, mock3],
    } = root;

    const child = cloneObject(mock1);
    const current = cloneObject(mock0);
    Definition.recursiveNodeCopyOverwriteElements(child, current);
    expect(current.elements[0].elements.length).toBe(3);

    Definition.recursiveNodeCopyOverwriteElements(mock3, mock2);
  });
});
