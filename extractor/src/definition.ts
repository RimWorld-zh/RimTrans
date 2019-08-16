import pth from 'path';
import fse from 'fs-extra';
import globby from 'globby';

import {
  ATTRIBUTE_NAME_NAME,
  ATTRIBUTE_NAME_PARENT_NAME,
  ATTRIBUTE_NAME_INHERIT,
  FIELD_NAME_DEF_NAME,
} from './constants';

import { ExtractorEventEmitter } from './extractor-event-emitter';
import { XElementData, loadXML } from './xml';
import { replaceListItem, cloneObject } from './object';

export interface DefsElementMap {
  [path: string]: XElementData;
}

interface InheritanceNode {
  root: XElementData;
  def: XElementData;
  resolvedDef?: XElementData;
  parent?: InheritanceNode;
  children: InheritanceNode[];
}

export class DefinitionExtractor {
  private readonly emitter: ExtractorEventEmitter;

  public constructor(emitter: ExtractorEventEmitter) {
    this.emitter = emitter;
  }

  // ----------------------------------------------------------------
  // Loading

  /**
   * Load all Defs file of the mod.
   * @param directory path to the directory 'Defs' of the mod
   */
  public async load(directory: string): Promise<DefsElementMap> {
    if (!(await fse.pathExists(directory))) {
      return {};
    }

    const files = await globby(['**/*.xml'], {
      cwd: directory,
      onlyFiles: true,
      caseSensitiveMatch: false,
    });

    const map: DefsElementMap = {};

    await Promise.all(
      files.map(async file => {
        map[file] = await loadXML(pth.join(directory, file));
      }),
    );

    return map;
  }

  // ----------------------------------------------------------------
  // Inheritance
  // RimWorld Assembly-CSharp.dll Verse/XmlInheritance.cs

  /**
   * Resolve the Defs inheritance.
   * @param defsElementMaps the array of `DefDocumentMap`, `[Core, ...Mods]`.
   */
  public resolveInheritance(defsElementMaps: DefsElementMap[]): void {
    if (defsElementMaps.length < 1) {
      throw new Error(`The argument 'maps' is a empty array.`);
    }

    const allNodes: InheritanceNode[] = [];
    const parentMaps: Record<string, Record<string, InheritanceNode>>[] = [];

    // register
    defsElementMaps.forEach(map => {
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
      const { def } = node;
      const parentName = def.attributes[ATTRIBUTE_NAME_PARENT_NAME];
      if (parentName) {
        node.parent = getParent(node, parentName);
        if (node.parent) {
          node.parent.children.push(node);
        } else {
          const defType = def.name;
          const defNameElement = def.elements.find(c => c.name === FIELD_NAME_DEF_NAME);
          const defName = defNameElement && defNameElement.value.trim();
          this.emitter.emit(
            'error',
            `${defType} "${defName}" parent "${parentName}" not found.`,
          );
        }
      }
    });

    // resolve
    allNodes
      .filter(node => !node.parent)
      .forEach(node => this.resolveInheritanceNodeRecursively(node));

    allNodes.forEach(node => {
      if (node.def !== node.resolvedDef) {
        replaceListItem(node.root.elements, node.def, node.resolvedDef);
      }
    });
  }

  public resolveInheritanceNodeRecursively(node: InheritanceNode): void {
    if (node.resolvedDef) {
      throw new Error('Resolve cyclic inheritance node.');
    }
    this.resolveXmlNodeFor(node);
    node.children.forEach(child => this.resolveInheritanceNodeRecursively(child));
  }

  public resolveXmlNodeFor(node: InheritanceNode): void {
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
    this.recursiveNodeCopyOverwriteElements(child, current);
    node.resolvedDef = current;
  }

  public recursiveNodeCopyOverwriteElements(
    child: XElementData,
    current: XElementData,
  ): void {
    const inherit = child.attributes[ATTRIBUTE_NAME_INHERIT];
    if (inherit && inherit.toLowerCase() === 'false') {
      current.childNodes = child.childNodes;
      current.elements = child.elements;
      current.value = child.value;
      return;
    }

    current.attributes = child.attributes;

    const childValue = child.value.trim();

    if (childValue) {
      current.childNodes = [{ nodeType: 'text', value: childValue }];
      current.elements = [];
      current.value = childValue;
      return;
    }

    if (child.elements.length === 0) {
      if (current.elements.length > 0) {
        current.childNodes = [];
        current.elements = [];
        current.value = '';
      }
      return;
    }

    child.elements.forEach(elChild => {
      if (elChild.name === 'li') {
        current.childNodes.push(elChild);
        current.elements.push(elChild);
      } else {
        const elCurrent = current.elements.find(el => el.name === elChild.name);
        if (elCurrent) {
          this.recursiveNodeCopyOverwriteElements(elChild, elCurrent);
        } else {
          current.childNodes.push(elChild);
          current.elements.push(elChild);
        }
      }
    });
    current.value = '';
  }
}
