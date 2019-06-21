import fs from 'fs';
import pth from 'path';
import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import { pathsDefs, defsFileCount, outputInheritedDefs } from './utils.test';
import { parseXML, saveXML } from './xml';
import {
  DefDocumentMap,
  load,
  resolveInheritance,
  resolveInheritanceNodeRecursively,
  resolveXmlNodeFor,
  recursiveNodeCopyOverwriteElements,
} from './definition';
import { cloneObject } from './object';

describe('def', () => {
  let defMaps: DefDocumentMap[];
  beforeAll(async () => {
    defMaps = await load(pathsDefs);
  });

  test('load', async () => {
    expect(Object.keys(defMaps[0]).length).toBe(defsFileCount);
  });

  test('resolveInheritance', async () => {
    // arguments error
    expect(() => resolveInheritance([])).toThrowError(/empty array/);

    // parent not found
    resolveInheritance([
      {
        'test.xml': parseXML(`
      <Defs>
        <MockDef ParentName="MockX"></MockDef>
      </Defs>`),
      },
    ]);

    // core
    const maps = await resolveInheritance(defMaps);
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
      resolveInheritanceNodeRecursively({
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
      resolveXmlNodeFor({
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
    recursiveNodeCopyOverwriteElements(child, current);
    expect(current.elements[0].elements.length).toBe(3);

    recursiveNodeCopyOverwriteElements(mock3, mock2);
  });
});
