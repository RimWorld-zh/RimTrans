import { genPathResolve } from '@huiji/shared-utils';
import { JSDOM } from 'jsdom';
import * as xml from './xml';

const resolvePath = genPathResolve(__dirname, '..', '..', 'Core');

describe('xml', () => {
  xml.mountDomPrototype();

  test('load', async () => {
    const pathBiomesCold = resolvePath('Defs', 'BiomeDefs', 'Biomes_Cold.xml');
    const { documentElement: root } = await xml.load(pathBiomesCold);

    expect(root.tagName).toBe('Defs');

    Array.from(root.children).forEach(c => {
      expect(c.tagName).toBe('BiomeDef');
    });
  });

  test('parse', () => {
    const content = '<Defs><MockDef/><MockDef/></Defs>';
    const doc = xml.parse(content);
    const { documentElement: root } = doc;

    expect(root.tagName).toBe('Defs');

    Array.from(root.children).forEach(c => {
      expect(c.tagName).toBe('MockDef');
    });

    expect(Array.from(root.children).length).toBe(2);

    expect(root.outerHTML).toBe(content);
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

  test('prototype', () => {
    xml.mountDomPrototype();
  });

  test('prototype removeAllAttributes', () => {
    const { documentElement: root } = xml.parse(
      '<Defs Name="MockName" ParentName="MockParentName"></Defs>',
    );
    expect(root.attributes.length).toBe(2);
    root.removeAllAttributes();
    expect(root.attributes.length).toBe(0);
  });

  test('prototype value', () => {
    const doc = xml.parse(
      '<Defs><MockDef>MockValue</MockDef><MockDef> <A></A> </MockDef></Defs>',
    );
    const [def0, def1] = Array.from(doc.documentElement.children);

    expect(def0.elementValue).toBe('MockValue');

    def0.elementValue = 'MockingBird';
    expect(def0.elementValue).toBe('MockingBird');

    def0.removeAllChildNodes();
    expect(def0.elementValue).toBe('');

    def0.elementValue = 'MockingBird';
    expect(def0.elementValue).toBe('MockingBird');

    expect(def1.elementValue).toBe(' ');

    def1.elementValue = 'MockingBird';
    expect(def1.elementValue).toBe('MockingBird');
  });

  test('prototype appendChildClone', () => {
    const doc0 = xml.parse(
      '<Defs><MockDef0>MockValue</MockDef0><MockDef1></MockDef1></Defs>',
    );
    const doc1 = xml.parse(
      '<Defs><MockDef2>MockValue</MockDef2><MockDef3></MockDef3></Defs>',
    );
    const [def0, def1] = Array.from(doc0.documentElement.children);
    const [def2, def3] = Array.from(doc1.documentElement.children);
    def0.appendChildClone(def1);
    def0.appendChildClone(def2);
    def0.appendChildClone(def3);

    expect(def0.children[0].tagName).toBe(def1.tagName);
    expect(def0.children[0]).not.toBe(def1);
    expect(def0.children[1].tagName).toBe(def2.tagName);
    expect(def0.children[1]).not.toBe(def2);
  });

  test('prototype appendChildren', () => {
    const doc = xml.parse(
      '<Defs><MockDef0></MockDef0><MockDef1><A></A><B></B></MockDef1></Defs>',
    );
    const { documentElement: root } = doc;
    const [def0, def1] = Array.from(root.children);
    def0.appendChildren(def1.children);

    expect(def0.children[0].tagName).toBe('A');
    expect(def0.children[1].tagName).toBe('B');
    expect(def1.children[0]).toBeUndefined();
    expect(def1.children[1]).toBeUndefined();
  });

  test('prototype appendChildrenClone', () => {
    const doc = xml.parse(
      '<Defs><MockDef0></MockDef0><MockDef1><A></A><B></B></MockDef1></Defs>',
    );
    const { documentElement: root } = doc;
    const [def0, def1] = Array.from(root.children);
    def0.appendChildrenClone(def1.children);

    expect(def0.children[0].tagName).toBe('A');
    expect(def0.children[1].tagName).toBe('B');
    expect(def1.children[0].tagName).toBe('A');
    expect(def1.children[1].tagName).toBe('B');
    expect(def0.children[0]).not.toBe(def1.children[0]);
    expect(def0.children[1]).not.toBe(def1.children[1]);
  });

  test('prototype removeAllChildNodes', () => {
    const { documentElement: root } = xml.parse(
      '<Defs><MockDef></MockDef><MockDef></MockDef></Defs>',
    );
    expect(root.childNodes.length).toBeGreaterThan(0);
    root.removeAllChildNodes();
    expect(root.childNodes.length).toBe(0);
  });

  test('prototype element & elements', () => {
    const { documentElement: root } = xml.parse(
      '<Defs><MockDef0><a></a><b></b></MockDef0><MockDef1></MockDef1></Defs>',
    );
    {
      const first = root.getElement();
      expect(first.tagName).toBe('MockDef0');
      expect(first).toBe(root.children[0]);
    }
    {
      const def0 = root.getElement('MockDef0');
      expect(def0.tagName).toBe('MockDef0');
      expect(def0).toBe(root.children[0]);
    }
    {
      const def1 = root.getElement('MockDef1');
      expect(def1.tagName).toBe('MockDef1');
      expect(def1).toBe(root.children[1]);
    }
    {
      const elements = root.getElements();
      const nodes = Array.from(root.childNodes);

      expect(elements.length).toBe(2);
      expect(nodes.length).toBe(2);

      expect(elements[0] === nodes[0]).toBe(true);
      expect(elements[0].getElements()[0].tagName).toBe('a');
      expect(
        elements[0].getElements()[0] === (nodes[0] as Element).getElements()[0],
      ).toBe(true);

      expect(elements[0].tagName).toBe('MockDef0');
      expect(elements[0]).toBe(root.children[0]);
      expect(elements[1].tagName).toBe('MockDef1');

      expect(elements[1]).toBe(root.children[1]);
    }
    {
      const elements = root.getElements('MockDef0');
      expect(elements.length).toBe(1);
      expect(elements[0].tagName).toBe('MockDef0');
      expect(elements[0]).toBe(root.children[0]);
    }
    {
      const elements = root.getElements('MockDef1');
      expect(elements.length).toBe(1);
      expect(elements[0].tagName).toBe('MockDef1');
      expect(elements[0]).toBe(root.children[1]);
    }
  });
});
