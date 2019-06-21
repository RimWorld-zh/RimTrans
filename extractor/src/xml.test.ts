import * as io from '@rimtrans/io';
import { pathCore, pathTestMods } from './utils.test';
import { loadXML, parseXML } from './xml';

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
    const root = parseXML(`
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
    expect(root.childNodes.length).toBe(10);
    expect(root.elements.length).toBe(3);
    expect(root.elements[0].attributes.Name).toBe('MockBase');
    expect(root.elements[0].childNodes.length).toBe(1);
    expect(root.elements[1].attributes.ParentName).toBe('MockBase');
    expect(root.elements[1].childNodes.length).toBe(3);
  });
});
