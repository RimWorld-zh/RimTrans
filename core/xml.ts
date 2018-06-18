/**
 * Utils for xmljs
 */
// tslint:disable:no-reserved-keywords

import xmljs from 'xml-js';

/**
 * XML attributes
 */
export interface Attributes {
  [key: string]: string | number | undefined;
}

/**
 * XML node type.
 */
export type NodeType = 'comment' | 'element' | 'text';

/**
 * XML node.
 */
export type Node = Comment | Element | Text;

/**
 * XML comment node.
 */
export interface Comment {
  type: 'comment';
  comment: string;
}

/**
 * XML element node.
 */
export interface Element {
  type: 'element';
  name: string;
  attributes: Attributes;
  nodes: Node[];
}

/**
 * XML text node.
 */
export interface Text {
  type: 'text';
  text: string;
}

/**
 * Parse the xml document text and return the root element.
 * @param rawContent the plain text of the xml document.
 */
export function parse(rawContent: string): Element | undefined {
  const doc: Element = xmljs.xml2js(rawContent, {
    compact: false,
    trim: false,
    nativeType: false,
    addParent: false,
    alwaysArray: true,
    alwaysChildren: true,
    ignoreDeclaration: true,
    ignoreText: false,
    elementsKey: 'nodes',
  }) as Element;

  if (doc.nodes) {
    const root: Element | undefined = (doc.nodes as Element[]).find(
      n => n.type === 'element',
    );

    if (root) {
      (root.nodes as Element[]).forEach(resolveAttributes);
    }

    return root;
  }
}

function resolveAttributes(element: Element): void {
  if (element.type !== 'element') {
    return;
  }

  element.attributes = element.attributes || {};

  (element.nodes as Element[]).forEach(resolveAttributes);
}

/**
 * Clone a node, return a new instance.
 */
export function clone<T extends Node>(node: T): T {
  return JSON.parse(JSON.stringify(node));
}

/**
 * Covert to Element type, if incompatible return undefined.
 */
export function asElement(node: Node): Element | undefined {
  return node.type === 'element' ? (node as Element) : undefined;
}

/**
 * Covert to Comment type, if incompatible return undefined.
 */
export function asComment(node: Node): Comment | undefined {
  return node.type === 'comment' ? (node as Comment) : undefined;
}

/**
 * Covert to Text type, if incompatible return undefined.
 */
export function asText(node: Node): Text | undefined {
  return node.type === 'text' ? (node as Text) : undefined;
}

/**
 * Get the first child element of the element by the specified name.
 * @param element the element
 * @param name the name for the children of the element.
 */
export function getElement(element: Element, name: string): Element | undefined {
  return (element.nodes as Element[]).find(
    el => el.type === 'element' && el.name === name,
  );
}

/**
 * Get children elements of the element.
 */
export function getElements(element: Element): Element[];

/**
 * Get children elements of the element by the specified name.
 */
export function getElements(element: Element, name: string): Element[];

export function getElements(element: Element, name?: string): Element[] {
  return name
    ? (element.nodes as Element[]).filter(el => el.type === 'element' && el.name === name)
    : (element.nodes as Element[]).filter(el => el.type === 'element');
}

/**
 * Get the inner text of the element.
 */
export function getText(element: Element): string | undefined {
  const text: Text | undefined = (element.nodes as Text[]).find(n => n.type === 'text');

  return text ? text.text : undefined;
}
