import * as io from '@rimtrans/io';
import { pathCore, pathTestMods, resolvePath, TEMP } from './utils.test';
import { loadXML, parseXML, saveXML } from './xml';

describe('xml', () => {
  test('load', async () => {
    const ids = await io.search(['*'], { cwd: pathTestMods, onlyDirectories: true });
    await Promise.all(
      ids.slice(0, 10).map(async id => {
        const pathDefs = io.join(pathTestMods, id, 'Defs');
        if (await io.directoryExists(pathDefs)) {
          const defFiles = await io.search(['**/*.xml'], {
            cwd: pathDefs,
            case: false,
            onlyFiles: true,
          });
          Promise.all(
            defFiles.map(async file => {
              const root = await loadXML(io.join(pathDefs, file));
              expect(root.nodeType).toBe('element');
            }),
          );
        }
      }),
    );
  });

  test('parse', () => {
    const rootData = parseXML(`
    <Defs>
      <!---->
      <MockDef Name="MockBase"><defName>Mock_0</defName></MockDef>
      <MockDef ParentName="MockBase">
        <defName>Mock_1</defName>
      </MockDef>
      <MockDef disabled="">
        <defName></defName>
      </MockDef>
      <![CDATA[]]>
    </Defs>
    `);
    expect(rootData.childNodes.length).toBe(10);
    expect(rootData.elements.length).toBe(3);
    expect(rootData.elements[0].attributes.Name).toBe('MockBase');
    expect(rootData.elements[0].childNodes.length).toBe(1);
    expect(rootData.elements[1].attributes.ParentName).toBe('MockBase');
    expect(rootData.elements[1].childNodes.length).toBe(3);
  });

  test('save', async () => {
    const rootData = parseXML(`
    <Defs>
    <!---->
      <MockDef Name="MockBase"><defName>Mock_0</defName></MockDef>
      <MockDef ParentName="MockBase">
        <defName>Mock_1</defName>
      </MockDef>
      <MockDef disabled="">
        <defName></defName>
      </MockDef>
    </Defs>
    `);

    Promise.all([
      saveXML(resolvePath(TEMP, 'xml', '0.xml'), rootData, false),
      saveXML(resolvePath(TEMP, 'xml', '1.xml'), rootData, true),
      saveXML(resolvePath(TEMP, 'xml', '2.xml'), rootData, true, {
        printWidth: 120,
      }),
      saveXML(resolvePath(TEMP, 'xml', '3.xml'), rootData, true, {
        tabWidth: 4,
        endOfLine: 'cr',
      }),
      saveXML(resolvePath(TEMP, 'xml', '4.xml'), rootData, true, {
        useTabs: true,
        endOfLine: 'crlf',
      }),
    ]);
  });
});
