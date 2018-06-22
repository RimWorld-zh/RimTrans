/**
 * Definition
 */

import * as logger from './logger';
import * as xml from './xml';
import { stringCompare } from './utils';
import { schema } from './schema';

export interface DefinitionData {
  [defType: string]: xml.Element[];
}

/**
 * Parse the XML documents plain text to RimWorld Definition data.
 * @param rawContents key for file path, value for XML plain text.
 */
export function parse(rawContents: xml.RawContents): DefinitionData {
  const data: DefinitionData = {};
  // tslint:disable-next-line:typedef
  const addDefinition = (def: xml.Element): void => {
    if (!data[def.name]) {
      data[def.name] = [];
    }
    data[def.name].push(def);
  };

  Object.entries(rawContents)
    .sort((a, b) => stringCompare(a[0], b[0]))
    .forEach(([path, raw]) => {
      const root: xml.Element | undefined = xml.parse(raw, path);
      if (!root) {
        return;
      }

      let comment: string | undefined;
      let markDefs: { [defType: string]: boolean } = {};
      root.nodes.forEach(n => {
        const curComment: string | undefined = validComment(n);
        const def: xml.Element | undefined = xml.asElement(n);
        if (def) {
          if (curComment) {
            comment = curComment;
            markDefs = {};
            markDefs[def.name] = true;
            def.attributes.CommentBefore = comment;
          } else if (comment) {
            if (markDefs[def.name]) {
              comment = undefined;
              markDefs = {};
            } else {
              markDefs[def.name] = true;
              def.attributes.CommentBefore = comment;
            }
          }
          addDefinition(def);
        }
      });
    });

  return data;
}

function validComment(node: xml.Node): string | undefined {
  if (!node) {
    return undefined;
  }

  const comment: xml.Comment | undefined = xml.asComment(node);

  return comment &&
    comment.comment &&
    (comment.comment.match(/\r?\n/g) || []).length === 0
    ? comment.comment
    : undefined;
}

function isAbstract(def: xml.Element): boolean {
  return !!def.attributes && def.attributes.Abstract !== 'True';
}

// ======== Inheritance ========

interface InheritanceNode {
  def: xml.Element;
  // resolvedDef: xml.Element;
  // data: DefinitionData;
  parent?: string;
  children: InheritanceNode[];
}

interface NodeMap {
  [name: string]: InheritanceNode;
}

interface InheritanceMap {
  [defType: string]: NodeMap;
}

/**
 * Resolve the Defs inheritance.
 * @param dataList The Defs data of mods. Order by the load order.
 */
export function resolveInheritance(dataList: DefinitionData[]): void {
  const mapList: InheritanceMap[] = [];

  dataList.forEach(data => {
    const inheritanceMap: InheritanceMap = {};
    mapList.unshift(inheritanceMap);

    Object.entries(data).forEach(([defType, definitions]) => {
      const nodeMap: NodeMap = {};
      inheritanceMap[defType] = nodeMap;

      definitions.filter(def => def.attributes.Name).forEach(
        def =>
          (nodeMap[def.attributes.Name as string] = {
            def,
            children: [],
          }),
      );

      // tslint:disable-next-line:typedef
      const getParentNode = (def: xml.Element): InheritanceNode | undefined => {
        if (!def.attributes.ParentName) {
          return undefined;
        }

        let parentNode: InheritanceNode | undefined;
        for (const map of mapList) {
          if (map[defType] && map[defType][def.attributes.ParentName]) {
            parentNode = map[defType][def.attributes.ParentName];
            break;
          }
        }

        return parentNode;
      };

      definitions
        .filter(def => def.attributes.Name && def.attributes.ParentName)
        .map<[InheritanceNode, InheritanceNode | undefined]>(def => [
          nodeMap[def.attributes.Name as string],
          getParentNode(def),
        ])
        .forEach(([node, parent]) => {
          if (parent) {
            node.parent = parent.def.attributes.Name as string;
            parent.children.push(node);
          }
        });

      definitions
        .filter(def => !def.attributes.Name && def.attributes.ParentName)
        .forEach(def => {
          const parent: InheritanceNode | undefined = getParentNode(def);
          if (parent) {
            parent.children.push({
              def,
              parent: parent.def.attributes.Name as string,
              children: [],
            });
          }
        });
    });
  });

  mapList.forEach(map =>
    Object.entries(map).forEach(([defType, nodeMap]) => {
      Object.values(nodeMap)
        .filter(node => !node.parent)
        .forEach(resolveNodeRecursively);
    }),
  );
}

function resolveNodeRecursively(node: InheritanceNode): void {
  node.children.forEach(child => {
    child.def.attributes.Inherited = 'True';
    elementInheritRecursively(child.def, node.def);
    resolveNodeRecursively(child);
  });
}

function elementInheritRecursively(child: xml.Element, parent: xml.Element): void {
  if (child.attributes.Inherit === 'false') {
    return;
  }

  if (child.nodes.every(xml.isElementByName('li'))) {
    parent.nodes
      .filter(xml.isElementByName('li'))
      .forEach(elP => child.nodes.push(xml.clone(elP)));

    return;
  }

  parent.nodes.filter(xml.isElement).forEach(elP => {
    const elC: xml.Element | undefined = child.nodes.find(xml.isElementByName(elP.name));
    if (elC) {
      if (!elC.nodes.every(xml.isText) && !elP.nodes.every(xml.isText)) {
        elementInheritRecursively(elC, elP);
      }
    } else {
      child.nodes.push(xml.clone(elP));
    }
  });
}

// ======== Post process ========

export function resolveListItemIndex(element: xml.Element): void {
  element.nodes
    .filter(xml.isElementByName('li'))
    .forEach((li, index) => (li.attributes.Index = index));

  element.nodes.filter(xml.isElement).forEach(resolveListItemIndex);
}

function resolveDefaultValue(def: xml.Element): void {
  // tslint:disable-next-line:typedef no-any
  const schemaDefinition = (schema as any)[def.name];
  if (schemaDefinition) {
    Object.entries(schemaDefinition).forEach(([name, value]) => {
      if (typeof value === 'string' && !def.nodes.some(xml.isElementByName(name))) {
        (def.nodes as xml.Element[]).push({
          type: 'element',
          attributes: {},
          name,
          nodes: [
            {
              type: 'text',
              text: value,
            },
          ],
        });
      }
    });
  }
}

export function resolveMustTranslateFields(def: xml.Element): void {}
