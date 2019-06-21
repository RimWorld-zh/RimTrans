import { JSDOM } from 'jsdom';

export const DEFAULT_DECLARATION = '<?xml version="1.0" encoding="utf-8" ?>';

type XNodeData = XTextData | XCommentData | XElementData;

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

export async function loadXML(path: string): Promise<XElementData> {
  const {
    window: {
      document: { documentElement: root },
    },
  } = await JSDOM.fromFile(path, { contentType: 'text/xml' });

  return createElement(root);
}

export function parseXML(content: string): XElementData {
  const {
    window: {
      document: { documentElement: root },
    },
  } = new JSDOM(content, { contentType: 'text/xml' });

  return createElement(root);
}
