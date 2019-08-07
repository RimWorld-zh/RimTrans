import fs from 'fs';
import pth from 'path';
import { genPathResolve } from '@huiji/shared-utils';
import * as io from '@rimtrans/io';
import { pathsDefs, defsFileCount, outputInheritedDefs } from './utils.test';
import { ExtractorEventEmitter, ExtractorEventListener } from './extractor-event-emitter';
import { parseXML, saveXML } from './xml';
import { DefsElementMap, DefinitionExtractor } from './definition';
import { cloneObject } from './object';

describe('definition', () => {
  const emitter = new ExtractorEventEmitter();
  const definitionExtractor = new DefinitionExtractor(emitter);

  let defMaps: DefsElementMap[];

  beforeAll(async () => {
    defMaps = await Promise.all(pathsDefs.map(path => definitionExtractor.load(path)));
  });

  test('load', async () => {
    expect(Object.keys(defMaps[0]).length).toBe(defsFileCount);
  });

  test('resolveInheritance', async () => {
    // core
    definitionExtractor.resolveInheritance(defMaps);
    const core = defMaps[0];
    await io.deleteFileOrDirectory(outputInheritedDefs);
    for (const [path, root] of Object.entries(core)) {
      await saveXML(io.join(outputInheritedDefs, path), root, false);
    }
    expect(Object.keys(core).length).toBe(defsFileCount);
  });

  test('resolveInheritance arguments error', async () => {
    // arguments error
    try {
      await definitionExtractor.resolveInheritance([]);
    } catch (error) {
      expect(error).toBeTruthy();
    }
  });

  test('resolveInheritance parent not found', async () => {
    // parent not found
    let errorEmitted = false;

    let listener: ExtractorEventListener<string> = (event, error) => {
      expect(event).toBe('error');
      errorEmitted = true;

      expect(error.includes('not found')).toBe(true);
      expect(error.includes('MockDef')).toBe(true);
      expect(error.includes('undefined')).toBe(true);
      expect(error.includes('MockX')).toBe(true);
    };
    emitter.addListener('error', listener);
    await definitionExtractor.resolveInheritance([
      {
        'test.xml': parseXML(`
        <Defs>
          <MockDef ParentName="MockX"></MockDef>
        </Defs>`),
      },
    ]);
    emitter.removeListener('error', listener);

    listener = (event, error) => {
      expect(error.includes('MockDefABC'));
    };
    await definitionExtractor.resolveInheritance([
      {
        'test.xml': parseXML(`
        <Defs>
          <MockDef ParentName="MockX">
            <defName>MockDefABC</defName>
          </MockDef>
        </Defs>`),
      },
    ]);
    emitter.removeListener('error', listener);

    expect(errorEmitted).toBe(true);
  });

  test('resolveInheritanceNodeRecursively & resolveXmlNodeFor', async () => {
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

    await expect(() =>
      definitionExtractor.resolveInheritanceNodeRecursively({
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

    await expect(() =>
      definitionExtractor.resolveXmlNodeFor({
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
    definitionExtractor.recursiveNodeCopyOverwriteElements(child, current);
    expect(current.elements[0].elements.length).toBe(3);

    definitionExtractor.recursiveNodeCopyOverwriteElements(mock3, mock2);
  });
});
