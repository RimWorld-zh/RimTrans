/**
 * Utils for xmljs
 */
// tslint:disable:no-reserved-keywords

import xmljs from 'xml-js';
import * as logger from './logger';

export interface RawContents {
  [path: string]: string;
}

/**
 * XML attributes
 */
export interface Attributes {
  [key: string]: string | number | undefined;
  Name?: string;
  ParentName?: string;
  Abstract?: 'True';
  CommentBefore?: string;
  Index?: number;
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
 * @param content the plain text of the xml document.
 */
export function parse(content: string, path?: string): Element | undefined {
  let doc: Element;
  try {
    doc = xmljs.xml2js(content, {
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
  } catch (error) {
    logger.error(error);

    return;
  }

  if (doc.nodes) {
    const root: Element | undefined = (doc.nodes as Element[]).find(
      n => n.type === 'element',
    );

    if (root) {
      (root.nodes as Element[]).forEach(resolveAttributes);
    }

    return root;
  }

  logger.error(`Missing root element.\nfile: "${path}"`);

  return undefined;
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

// region ======== For Comment ========

export function isComment(node: Node): node is Comment {
  return node.type === 'comment';
}

export function asComment(node: Node): Comment | undefined {
  return isComment(node) ? node : undefined;
}

// endregion

// region ======== For Element ========

export function isElement(node: Node): node is Element {
  return node.type === 'element';
}

export function isElementByName(name: string): (node: Node) => node is Element {
  return (node): node is Element => node.type === 'element' && node.name === name;
}

export function asElement(node: Node): Element | undefined {
  return isElement(node) ? node : undefined;
}

// endregion

// region ======== For Text ========

export function isText(node: Node): node is Text {
  return node.type === 'text';
}

export function asText(node: Node): Text | undefined {
  return isText(node) ? node : undefined;
}

export function getText(element: Element): string | undefined {
  const text: Text | undefined = element.nodes.find(isText);
  if (text) {
    return text.text;
  }

  return undefined;
}

export function setText(element: Element, value: string): void {
  const text: Text | undefined = element.nodes.find(isText);
  if (text) {
    text.text = value;
  }
}

// endregion
