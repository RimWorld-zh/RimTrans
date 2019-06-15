import fs from 'fs';
import pth from 'path';
import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import * as xml from './xml';
import {
  load,
  resolveInheritance,
  resolveInheritanceNodeRecursively,
  resolveXmlNodeFor,
  recursiveNodeCopyOverwriteElements,
} from './def';

const resolvePath = genPathResolve(__dirname, '..', '..');

const pathDefs = resolvePath('Core', 'Defs');
const defsFileCount = 413;

describe('def', () => {
  test('load', async () => {
    const map = await load(pathDefs);
    expect(Object.keys(map).length).toBe(defsFileCount);
  });

  test('resolveInheritance', async () => {
    // arguments error
    expect(() => resolveInheritance([])).toThrowError(/empty array/);

    // parent not found
    resolveInheritance([
      {
        'test.xml': xml.parse(`
      <Defs>
        <MockDef ParentName="MockX"></MockDef>
      </Defs>`),
      },
    ]);

    // core
    const maps = await Promise.all([load(pathDefs)]).then(resolveInheritance);
    const core = maps[0];
    await io.deleteFileOrDirectory(resolvePath('.tmp', 'core-inherited-defs'));
    await Promise.all(
      Object.entries(core).map(async ([path, defs]) => {
        const doc = xml.create('Defs');
        defs.forEach(def => doc.documentElement.appendChild(def));
        await io.save(
          resolvePath('.tmp', 'core-inherited-defs', path),
          doc.documentElement.outerHTML,
        );
      }),
    );
    expect(Object.keys(core).length).toBe(defsFileCount);
  });

  test('resolveInheritanceNodeRecursively & resolveXmlNodeFor', () => {
    const {
      documentElement: {
        children: [mock0, mock1],
      },
    } = xml.parse(`
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

    expect(() =>
      resolveInheritanceNodeRecursively({
        def: mock1,
        resolvedDef: mock1,
        parent: {
          def: mock0,
          resolvedDef: mock0,
          children: [],
        },
        children: [],
      }),
    ).toThrowError(/cyclic/);

    expect(() =>
      resolveXmlNodeFor({
        def: mock1,
        parent: {
          def: mock0,
          children: [],
        },
        children: [],
      }),
    ).toThrowError(/not been resolved yet/);
  });

  test('recursiveNodeCopyOverwriteElements', () => {
    const {
      documentElement: {
        children: [mock0, mock1, mock2, mock3],
      },
    } = xml.parse(`
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

    const current = mock1.ownerDocument.importNode(mock0, true);
    recursiveNodeCopyOverwriteElements(mock1, current);
    expect(current.children[0].children.length).toBe(3);

    recursiveNodeCopyOverwriteElements(mock3, mock2);
  });
});
