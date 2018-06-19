/**
 * Definition
 */

import * as logger from './logger';
import * as xml from './xml';
import { stringCompare } from './utils';

export interface RawContents {
  [path: string]: string;
}

export interface DefinitionData {
  [defType: string]: xml.Element[];
}

/**
 * Parse the XML documents plain text to RimWorld Definition data.
 * @param rawContents key for file path, value for XML plain text.
 */
export function parse(rawContents: RawContents): DefinitionData {
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

      (root.nodes.filter(n => n.type === 'element') as xml.Element[]).forEach(def => {
        def.attributes.FilePath = path;
        addDefinition(def);
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
  if (
    child.attributes.Inherit &&
    (child.attributes.Inherit as string).toLowerCase() === 'false'
  ) {
    return;
  }

  if (xml.isAllElementIs(child, 'li')) {
    xml.getElements(parent, 'li').forEach(elP => child.nodes.push(xml.clone(elP)));

    return;
  }

  xml.getElements(parent).forEach(elP => {
    const elC: xml.Element | undefined = xml.getElement(child, elP.name);
    if (elC) {
      if (!xml.isEndingNode(elC) && !xml.isEndingNode(elP)) {
        elementInheritRecursively(elC, elP);
      }
    } else {
      child.nodes.push(xml.clone(elP));
    }
  });
}
