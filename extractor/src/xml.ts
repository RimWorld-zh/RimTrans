import { JSDOM } from 'jsdom';
import prettier, { Options } from 'prettier';
import * as io from '@rimtrans/io';

export const DEFAULT_DECLARATION = '<?xml version="1.0" encoding="utf-8" ?>';

export type XNodeData = XTextData | XCommentData | XElementData;

export interface XTextData {
  nodeType: 'text';
  value: string;
}
export interface XCommentData {
  nodeType: 'comment';
  value: string;
}

export interface XElementData {
  nodeType: 'element';
  name: string;
  attributes: Record<string, string | undefined>;
  childNodes: XNodeData[];
  elements: XElementData[];
  value: string;
}

function createElement(element: Element): XElementData {
  const name = element.tagName;

  const attributes: Record<string, string> = {};
  const { length: attributesLength } = element.attributes;
  for (let i = 0; i < attributesLength; i++) {
    const attr = element.attributes[i];
    attributes[attr.name] = attr.value || '';
  }

  const childNodes: XNodeData[] = [];
  const elements: XElementData[] = [];
  const { length: nodesLength } = element.childNodes;
  let text: Text | undefined;
  let elem: XElementData;
  for (let i = 0; i < nodesLength; i++) {
    const node = element.childNodes[i];
    switch (node.nodeType) {
      case node.TEXT_NODE:
        text = node as Text;
        childNodes.push({ nodeType: 'text', value: node.nodeValue as string });
        break;

      case node.ELEMENT_NODE:
        elem = createElement(node as Element);
        childNodes.push(elem);
        elements.push(elem);
        break;

      case node.COMMENT_NODE:
        childNodes.push({ nodeType: 'comment', value: node.nodeValue || '' });
        break;

      default:
    }
  }
  const value: string = (text && text.nodeValue) || '';

  return {
    nodeType: 'element',
    name,
    attributes,
    childNodes,
    elements,
    value,
  };
}

/**
 * Load XML document from a file.
 * @param path the path to the XML document file.
 */
export async function loadXML(path: string): Promise<XElementData> {
  const {
    window: {
      document: { documentElement: root },
    },
  } = await JSDOM.fromFile(path, { contentType: 'text/xml' });

  return createElement(root);
}

/**
 * Get XML document by parsing text.
 * @param content the XML document text
 */
export function parseXML(content: string): XElementData {
  const {
    window: {
      document: { documentElement: root },
    },
  } = new JSDOM(content, { contentType: 'text/xml' });

  return createElement(root);
}

function append(doc: Document, parent: Element, data: XElementData): Element {
  const elem = doc.createElement(data.name);

  Object.entries(data.attributes).forEach(([name, value]) =>
    elem.setAttribute(name, value as string),
  );

  data.childNodes.forEach(node => {
    switch (node.nodeType) {
      case 'element':
        append(doc, elem, node);
        break;

      case 'comment':
        elem.appendChild(doc.createComment(node.value));
        break;

      default:
        elem.appendChild(doc.createTextNode(node.value));
        break;
    }
  });

  parent.appendChild(elem);

  return elem;
}

export type PrettierOptions = Options;

export interface DefaultPrettierOptions {
  parser: PrettierOptions['parser'];
  printWidth: number;
  tabWidth: number;
  useTabs: boolean;
  endOfLine: PrettierOptions['endOfLine'];
  htmlWhitespaceSensitivity: PrettierOptions['htmlWhitespaceSensitivity'];
}

export function defaultXmlPrettierOptions(): DefaultPrettierOptions {
  return {
    parser: 'html',
    printWidth: 2000,
    tabWidth: 2,
    useTabs: false,
    endOfLine: 'lf',
    htmlWhitespaceSensitivity: 'ignore',
  };
}

export async function saveXML(
  path: string,
  rootData: XElementData,
  format?: boolean,
  prettierOptions?: PrettierOptions,
): Promise<void> {
  const {
    window: { document: doc },
  } = new JSDOM(`<body/>`, { contentType: 'text/xml' });

  const root = append(doc, doc.documentElement, rootData);

  const resolvedOptions: PrettierOptions = {
    ...defaultXmlPrettierOptions(),
    ...prettierOptions,
  };
  const { endOfLine } = resolvedOptions;
  const eol = (endOfLine === 'cr' && '\r') || (endOfLine === 'crlf' && '\r\n') || '\n';

  const content = format
    ? prettier.format(root.outerHTML, resolvedOptions)
    : root.outerHTML;
  await io.save(path, `${DEFAULT_DECLARATION}${eol}${content}${eol}`);
}
