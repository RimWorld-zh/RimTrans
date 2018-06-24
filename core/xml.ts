/**
 * Scan Keyed
 */
// tslint:disable:no-reserved-keywords

import fs, { stat } from 'fs';
import sax from 'sax';
import Stack from './stack';

export interface Document {
  doctype: string;
  root: Element;
}

export type Node = Comment | Element | Text;

export interface Attributes {
  [name: string]: string | number | boolean | undefined;
  Name?: string;
  ParentName?: string;
  Abstract?: string;
  Inherit?: string;
  Path?: string;
  Index?: number;
  Comment?: string;
}

export interface Comment {
  type: 'comment';
  value: string;
}

export interface Element {
  type: 'element';
  name: string;
  attributes: Attributes;
  value?: string;
  nodes: Node[];
}

export interface Text {
  type: 'text';
  value: string;
}

const strict: boolean = true;
const options: object = {
  trim: false,
  normalize: false,
  lowercase: false,
  xmlns: false,
  position: true,
  strictEntities: true,
};
const parser: sax.SAXParser = sax.parser(strict, options);

/**
 * Parse the xml string and return the root element.
 * @param content plain text for xml document
 */
export function parse(content: string): Element {
  const stack: Stack<Element> = new Stack<Element>();

  let current: Element = stack.push({
    type: 'element',
    name: 'doc',
    attributes: {},
    nodes: [],
  }).peek;

  parser.onerror = error => {
    throw error;
  };

  parser.onopentag = (tag: sax.Tag) => {
    const element: Element = {
      type: 'element',
      name: tag.name,
      attributes: { ...tag.attributes },
      nodes: [],
    };
    current.nodes.push(element);
    if (!tag.isSelfClosing) {
      current = stack.push(element).peek;
    }
  };
  parser.oncomment = comment => {
    current.nodes.push({
      type: 'comment',
      value: comment,
    });
  };
  parser.ontext = text => {
    current.nodes.push({
      type: 'text',
      value: text,
    });
  };
  parser.onclosetag = tagName => {
    if (!current.nodes.some(isElement)) {
      if (current.nodes.length === 1 && isText(current.nodes[0])) {
        current.value = current.nodes[0].value;
        current.nodes = [];
      } else {
        current.value = (current.nodes as (Text | Comment)[])
          .map(n => (isText(n) ? n.value : ''))
          .join('');
      }
    }
    current = stack.pop().peek;
  };

  parser.write(content).close();

  return current.nodes.find(isElement) as Element;
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

export function createElement(name: string): Element;
export function createElement(name: string, value: string): Element;
export function createElement(name: string, children: Node[]): Element;
export function createElement(name: string, attributes: Attributes): Element;
export function createElement(
  name: string,
  attributes: Attributes,
  value: string,
): Element;
export function createElement(
  name: string,
  attributes: Attributes,
  children: Node[],
): Element;
export function createElement(
  name: string,
  ...args: (string | Node[] | Attributes)[]
): Element {
  if (args.length === 1) {
    // TODO
  } else if (args.length === 2) {
    // TODO
  }

  return {
    type: 'element',
    attributes: {},
    name,
    nodes: [],
  };
}

// endregion

// region ======== For Text ========

export function isText(node: Node): node is Text {
  return node.type === 'text';
}

export function asText(node: Node): Text | undefined {
  return isText(node) ? node : undefined;
}

export function getChildElementText(element: Element, child: string): string | undefined {
  const childElement: Element | undefined = element.nodes.find(isElementByName(child));

  return childElement ? childElement.value : undefined;
}

// endregion
