import pth from 'path';
import globby from 'globby';
import { getFiles } from './io';
import * as xml from './xml';

/**
 * Load all Defs file from a directory.
 * @param path the path to the `Defs` directory.
 */
export async function load(path: string): Promise<Record<string, XMLDocument>> {
  const map: Record<string, XMLDocument> = {};
  await getFiles(['**/*.xml'], {
    cwd: path,
    case: false,
    onlyFiles: true,
  }).then(files =>
    Promise.all(
      files.map(async file => {
        map[file] = await xml.load(pth.join(path, file));
      }),
    ),
  );
  return map;
}

// ----------------------------------------------------------------
// Inheritance
// RimWorld Assembly-CSharp.dll Verse/XmlInheritance.cs

const ATTRIBUTE_NAME_NAME = 'Name';
const ATTRIBUTE_NAME_PARENT_NAME = 'ParentName';
const ATTRIBUTE_NAME_INHERIT = 'Inherit';

interface InheritanceNode {
  def: Element;
  resolvedDef?: Element;
  parent?: InheritanceNode;
  children: InheritanceNode[];
}

/**
 * Resolve the Defs inheritance.
 * @param maps the array of `DefDocumentMap`, `[current, ...dependencies, core]`.
 */
export function resolveInheritance(
  maps: Record<string, XMLDocument>[],
): Record<string, Element[]>[] {
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
        const {
          documentElement: { children: defs },
        } = map[path];
        Array.from(defs).forEach(def => {
          const node: InheritanceNode = {
            def,
            children: [],
          };
          allNodes.push(node);
          const nameAttribute = def.attributes.getNamedItem(ATTRIBUTE_NAME_NAME);
          const subMap = parentMap[def.tagName] || (parentMap[def.tagName] = {});
          if (nameAttribute && nameAttribute.value && !subMap[nameAttribute.value]) {
            subMap[nameAttribute.value] = node;
          }
        });
      });
    parentMaps.push(parentMap);
  });

  const getParent = (
    node: InheritanceNode,
    parentName: string,
  ): InheritanceNode | undefined => {
    const defType = node.def.tagName;
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
    const parentNameAttribute = node.def.attributes.getNamedItem(
      ATTRIBUTE_NAME_PARENT_NAME,
    );
    if (parentNameAttribute && parentNameAttribute.value) {
      node.parent = getParent(node, parentNameAttribute.value);
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

  return maps.map((m, i) => {
    const map: Record<string, Element[]> = {};

    // TODO detect undefined
    Object.entries(m).forEach(([path, doc]) => {
      map[path] = Array.from(doc.documentElement.children)
        .map(def => allNodes.find(n => n.def === def))
        .filter((node): node is InheritanceNode => !!node)
        .map(node => node.resolvedDef)
        .filter((def): def is Element => !!def);
    });

    return map;
  });
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
  const current = (node.def.ownerDocument as Document).importNode(
    node.parent.resolvedDef,
    true,
  );
  recursiveNodeCopyOverwriteElements(node.def, current);
  node.resolvedDef = current;
}

export function recursiveNodeCopyOverwriteElements(
  child: Element,
  current: Element,
): void {
  const inheritAttribute = child.attributes.getNamedItem(ATTRIBUTE_NAME_INHERIT);
  if (
    inheritAttribute &&
    inheritAttribute.value &&
    inheritAttribute.value.toLowerCase() === 'false'
  ) {
    Array.from(current.childNodes).forEach(c => current.removeChild(c));
    Array.from(child.childNodes).forEach(c => current.appendChild(c));
  } else {
    Array(current.attributes.length)
      .fill(0)
      .map((v, i) => current.attributes.item(i) as Attr)
      .forEach(attr => current.attributes.removeNamedItem(attr.name));
    Array(child.attributes.length)
      .fill(0)
      .forEach((v, i) => {
        const childAttribute = child.attributes.item(i) as Attr;
        const currentAttribute = (current.ownerDocument as Document).createAttribute(
          childAttribute.name,
        );
        currentAttribute.value = childAttribute.value;
        current.attributes.setNamedItem(currentAttribute);
      });

    const list: Element[] = [];
    let textNode: Node | undefined;
    Array.from(child.childNodes).forEach(node => {
      switch (node.nodeType) {
        case node.TEXT_NODE:
          textNode = node;
          break;
        case node.ELEMENT_NODE:
          list.push(node as Element);
          break;
        default:
      }
    });

    if (textNode && textNode.nodeValue && textNode.nodeValue.trim()) {
      // Remove all current nodes
      Array.from(current.childNodes).forEach(c => current.removeChild(c));
      current.appendChild((current.ownerDocument as Document).importNode(textNode, true));
    } else if (list.length === 0) {
      if (current.children.length > 0) {
        Array.from(current.childNodes).forEach(c => current.removeChild(c));
      }
    } else {
      list.forEach(elChild => {
        if (elChild.tagName === 'li') {
          current.appendChild(
            (current.ownerDocument as Document).importNode(elChild, true),
          );
        } else {
          const elCurrent = Array.from(current.children).find(
            el => el.tagName === elChild.tagName,
          );
          if (elCurrent) {
            recursiveNodeCopyOverwriteElements(elChild, elCurrent);
          } else {
            current.appendChild(
              (current.ownerDocument as Document).importNode(elChild, true),
            );
          }
        }
      });
    }
  }
}
