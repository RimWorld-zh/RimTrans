// tslint:disable:no-reserved-keywords

import fs, { stat } from 'fs';
import sax from 'sax';
import Stack from '../common/stack';

export type Node = Comment | Element | Text;

export interface Attributes {
  [name: string]: string | number | boolean | undefined;
  // attributes for RimWorld
  Name?: string;
  ParentName?: string;
  Abstract?: string;
  Inherit?: string;
  // attributes for RimTrans
  Instanced?: boolean;
  Path?: string;
  FileName?: string;
  Index?: number;
  Comment?: string;
  SourceDefType?: string;
  SourceDef?: string;
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
  __metadata?: Attributes;
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

/**
 * Parse the xml string and return the root element.
 * @param content plain text for xml document
 */
export function parse(content: string): Element {
  const parser: sax.SAXParser = sax.parser(strict, options);
  const stack: Stack<Element> = new Stack<Element>();

  let current: Element = stack.push(createElement('doc')).peek;

  // parser.onerror = error => {
  //   throw error;
  // };

  parser.onopentag = (tag: sax.Tag) => {
    // console.log('onopentag', parser.line, parser.column, tag.name);
    const element: Element = createElement(tag.name, { ...tag.attributes });
    element.__metadata = {
      line: parser.line,
      column: parser.column,
    };
    current.nodes.push(element);
    current = stack.push(element).peek;
  };
  parser.oncomment = comment => {
    // console.log('oncomment', parser.line, parser.column);
    current.nodes.push({
      type: 'comment',
      value: comment,
    });
  };
  parser.ontext = text => {
    // console.log('ontext', parser.line, parser.column);
    current.nodes.push({
      type: 'text',
      value: text,
    });
  };
  parser.onclosetag = tagName => {
    // console.log('onclosetag', parser.line, parser.column, tagName);
    if (!current.nodes.some(isElement)) {
      if (current.nodes.length === 1 && isText(current.nodes[0])) {
        current.value = current.nodes[0].value;
        current.nodes = [];
      } else if (current.nodes.length > 1) {
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
  let value: string | undefined;
  let attributes: Attributes = {};
  let nodes: Node[] = [];
  if (args.length === 1) {
    if (typeof args[0] === 'string') {
      value = args[0] as string;
    } else if (Array.isArray(args[0])) {
      nodes = args[0] as Node[];
    } else if (typeof args[0] === 'object') {
      attributes = args[0] as Attributes;
    }
  } else if (args.length === 2) {
    attributes = args[0] as Attributes;
    if (typeof args[1] === 'string') {
      value = args[1] as string;
    } else if (Array.isArray(args[1])) {
      nodes = args[1] as Node[];
    }
  }

  return {
    type: 'element',
    attributes,
    name,
    value,
    nodes,
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
