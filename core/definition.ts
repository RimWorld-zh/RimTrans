/**
 * Definition
 */

import * as logger from './logger';
import * as xml from './xml';
import { stringCompare, RawContents } from './utils';
import { schema } from './schema';

/**
 * RimWorld Defs Data, key for def type, value for defs
 */
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

  const rootMap: { [path: string]: xml.Element } = {};

  Object.entries(rawContents)
    .sort((a, b) => stringCompare(a[0], b[0]))
    .map(([path, content]) => {
      try {
        const root: xml.Element = xml.parse(content);
        root.attributes.Path = path;

        return root;
      } catch (error) {
        logger.error(`Failed to parse Defs file: "${path}".\n${(error as Error).stack}`);
      }

      return undefined;
    })
    .forEach(root => {
      if (!root) {
        return;
      }

      let comment: string | undefined;
      let markDefs: { [defType: string]: boolean } = {};
      root.nodes.forEach((node, index) => {
        const curComment: string | undefined = validComment(node);
        const def: xml.Element | undefined = xml.asElement(node);
        if (def) {
          if (curComment) {
            comment = curComment;
            markDefs = {};
            markDefs[def.name] = true;
            def.attributes.Comment = comment;
          } else if (comment && !markDefs[def.name]) {
            markDefs[def.name] = true;
            def.attributes.Comment = comment;
          }
          def.attributes.Index = index;
          addDefinition(def);
        }
      });
    });

  return data;
}

// ======== Utils ========

function validComment(node: xml.Node): string | undefined {
  if (!node) {
    return undefined;
  }

  const comment: xml.Comment | undefined = xml.asComment(node);

  return comment && comment.value && (comment.value.match(/\r?\n/g) || []).length === 0
    ? comment.value
    : undefined;
}

export function getDefName(def: xml.Element): string | undefined {
  return xml.getChildElementText(def, 'defName');
}

export function isAbstract(def: xml.Element): boolean {
  return !!def.attributes && def.attributes.Abstract !== 'True';
}

function insertDefAfter(defs: xml.Element[], that: xml.Element, def: xml.Element): void {
  defs.splice(defs.indexOf(that) + 1, 0, def);
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

export function postProcess(dataList: DefinitionData[]): void {
  dataList.forEach(resolveListItemIndex);
}

// ==== List Item ====

function resolveListItemIndexRecursively(element: xml.Element): void {
  element.nodes
    .filter(xml.isElementByName('li'))
    .forEach((li, index) => (li.attributes.Index = index));

  element.nodes.filter(xml.isElement).forEach(resolveListItemIndexRecursively);
}

function resolveListItemIndex(data: DefinitionData): void {
  Object.entries(data).forEach(([defType, defs]) =>
    defs.forEach(resolveListItemIndexRecursively),
  );
}

// ==== ThingDefGenerator_Building ====

const BlueprintDefNamePrefix: string = 'Blueprint_';
const InstallBlueprintDefNamePrefix: string = 'Install_';
const BuildingFrameDefNamePrefix: string = 'Frame_';

function isBuildableByPlayer(def: xml.Element): boolean {
  return def.nodes.some(xml.isElementByName('designationCategory'));
}

function isMinifiable(def: xml.Element): boolean {
  return def.nodes.some(xml.isElementByName('minifiedDef'));
}

function generateThingDef_Building(dataList: DefinitionData[]): void {
  dataList.forEach(data => {
    if (data.ThingDef) {
      data.ThingDef.forEach(def => {
        if (isBuildableByPlayer(def)) {
          //
        }
        if (isMinifiable(def)) {
          //
        }
      });
    }
    if (data.TerrainDef) {
      data.TerrainDef.forEach(def => {
        if (isBuildableByPlayer(def)) {
          //
        }
      });
    }
  });
}
