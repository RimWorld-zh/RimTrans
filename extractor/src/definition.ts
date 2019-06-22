import pth from 'path';
import * as io from '@rimtrans/io';
import { XElementData, loadXML } from './xml';
import { replaceListItem, cloneObject } from './object';
import {
  ATTRIBUTE_NAME_NAME,
  ATTRIBUTE_NAME_PARENT_NAME,
  ATTRIBUTE_NAME_INHERIT,
} from './constants';

export interface DefDocumentMap {
  [path: string]: XElementData;
}

// ----------------------------------------------------------------
// Loading

/**
 * Load all Defs file from a directory and get array of `DefDocumentMap`.
 * @param paths the array of paths to Def directories, order: `[core, ...mods]`.
 */
export async function load(paths: string[]): Promise<DefDocumentMap[]> {
  return Promise.all(
    paths.map(async dir => {
      const map: DefDocumentMap = {};
      if (!(await io.directoryExists(dir))) {
        return map;
      }

      await io
        .search(['**/*.xml'], {
          cwd: dir,
          case: false,
          onlyFiles: true,
        })
        .then(files =>
          Promise.all(
            files.map(async file => {
              map[file] = await loadXML(pth.join(dir, file));
            }),
          ),
        );
      return map;
    }),
  );
}

// ----------------------------------------------------------------
// Inheritance
// RimWorld Assembly-CSharp.dll Verse/XmlInheritance.cs

interface InheritanceNode {
  root: XElementData;
  def: XElementData;
  resolvedDef?: XElementData;
  parent?: InheritanceNode;
  children: InheritanceNode[];
}

/**
 * Resolve the Defs inheritance.
 * @param maps the array of `DefDocumentMap`, order: `[core, ...mods]`.
 */
export function resolveInheritance(maps: DefDocumentMap[]): DefDocumentMap[] {
  if (maps.length < 1) {
    throw new Error(`The argument 'maps' is a empty array.`);
  }

  const allNodes: InheritanceNode[] = [];
  const parentMaps: Record<string, Record<string, InheritanceNode>>[] = [];

  // register
  maps.forEach(map => {
    const parentMap: Record<string, Record<string, InheritanceNode>> = {};
    Object.keys(map)
      .sort()
      .forEach(path => {
        const root = map[path];
        root.elements.forEach(def => {
          const node: InheritanceNode = {
            root,
            def,
            children: [],
          };
          allNodes.push(node);
          const name = def.attributes[ATTRIBUTE_NAME_NAME];
          const subMap = parentMap[def.name] || (parentMap[def.name] = {});
          if (name) {
            subMap[name] = node;
          }
        });
      });
    parentMaps.push(parentMap);
  });

  // Reverse parent map
  parentMaps.reverse();

  const getParent = (
    node: InheritanceNode,
    parentName: string,
  ): InheritanceNode | undefined => {
    const defType = node.def.name;
    // eslint-disable-next-line no-restricted-syntax
    for (const parentMap of parentMaps) {
      const parent = parentMap[defType] && parentMap[defType][parentName];
      if (parent) {
        return parent;
      }
    }
    return undefined;
  };

  // link parents and children
  allNodes.forEach(node => {
    const parentName = node.def.attributes[ATTRIBUTE_NAME_PARENT_NAME];
    if (parentName) {
      node.parent = getParent(node, parentName);
      if (node.parent) {
        node.parent.children.push(node);
      } else {
        // TODO parent not found
      }
    }
  });

  // resolve
  allNodes
    .filter(node => !node.parent)
    .forEach(node => resolveInheritanceNodeRecursively(node));

  allNodes.forEach(node => {
    if (node.def !== node.resolvedDef) {
      replaceListItem(node.root.elements, node.def, node.resolvedDef);
    }
  });

  return maps;
}

export function resolveInheritanceNodeRecursively(node: InheritanceNode): void {
  if (node.resolvedDef) {
    throw new Error('Resolve cyclic inheritance node.');
  }
  resolveXmlNodeFor(node);
  node.children.forEach(resolveInheritanceNodeRecursively);
}

export function resolveXmlNodeFor(node: InheritanceNode): void {
  if (!node.parent) {
    node.resolvedDef = node.def;
    return;
  }
  if (!node.parent.resolvedDef) {
    throw new Error(
      'Tried to resolve node whose parent has not been resolved yet. This means that this method was called in incorrect order.',
    );
  }

  const child = cloneObject(node.def);
  const current = cloneObject(node.parent.resolvedDef as XElementData);
  recursiveNodeCopyOverwriteElements(child, current);
  node.resolvedDef = current;
}

export function recursiveNodeCopyOverwriteElements(
  child: XElementData,
  current: XElementData,
): void {
  const inherit = child.attributes[ATTRIBUTE_NAME_INHERIT];
  if (inherit && inherit.toLowerCase() === 'false') {
    current.childNodes = child.childNodes;
    current.elements = child.elements;
    current.value = child.value;
  } else {
    current.attributes = child.attributes;

    const childValue = child.value.trim();

    if (childValue) {
      current.childNodes = [{ nodeType: 'text', value: childValue }];
      current.elements = [];
      current.value = childValue;
    } else if (child.elements.length === 0) {
      if (current.elements.length > 0) {
        current.childNodes = [];
        current.elements = [];
        current.value = '';
      }
    } else {
      child.elements.forEach(elChild => {
        if (elChild.name === 'li') {
          current.childNodes.push(elChild);
          current.elements.push(elChild);
        } else {
          const elCurrent = current.elements.find(el => el.name === elChild.name);
          if (elCurrent) {
            recursiveNodeCopyOverwriteElements(elChild, elCurrent);
          } else {
            current.childNodes.push(elChild);
            current.elements.push(elChild);
          }
        }
      });
      current.value = '';
    }
  }
}
