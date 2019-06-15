import fs from 'fs';
import { JSDOM } from 'jsdom';

export const DEFAULT_DECLARATION = '<?xml version="1.0" encoding="utf-8" ?>';

/**
 * Load xml document file.
 * @param path the path to xml document file.
 */
export async function load(path: string): Promise<XMLDocument> {
  const {
    window: { document },
  } = await JSDOM.fromFile(path, { contentType: 'text/xml' });
  return document as XMLDocument;
}

/**
 * Parse string text to xml document.
 * @param content the string text of the xml
 */
export function parse(content: string): XMLDocument {
  const {
    window: { document },
  } = new JSDOM(content, {
    contentType: 'text/xml',
  });

  return document as XMLDocument;
}

/**
 * Create a new xml document.
 * @param rootTagName the tag name of the root element.
 */
export function create(rootTagName: string): XMLDocument {
  return parse(`${DEFAULT_DECLARATION}<${rootTagName}/>`);
}

declare global {
  interface Element {
    /**
     * mark the JSDOM properties is mounted or not.
     */
    hasMounted: boolean;

    /**
     * Get or set the text value of the element.
     */
    elementValue: string;

    /**
     * Remove all attributes of the element, and return the element itself.
     */
    removeAllAttributes(): this;

    /**
     * Append a child node to the element, the node will be clone.
     * @param newChild the new child node
     */
    appendChildClone<T extends Node>(newChild: T): T;

    /**
     * Append some child nodes to the element, and return the element itself.
     * @param newChildren the new children, can be `Iterable` or `ArrayLike`
     */
    appendChildren<T extends Node>(newChildren: Iterable<T> | ArrayLike<T>): this;

    /**
     * Append some child nodes to the element, the child nodes will be clone, and return the element itself.
     * @param newChildren the new children, can be `Iterable` or `ArrayLike`
     */
    appendChildrenClone<T extends Node>(newChildren: Iterable<T> | ArrayLike<T>): this;

    /**
     * Remove all child nodes of the element, and return itself.
     */
    removeAllChildNodes(): this;

    /**
     * Get the first child element with the specified name.
     * @param name the name to match
     */
    getElement(name?: string): Element | undefined;

    /**
     * Get the child elements with the specified name.
     * @param name the name to match
     */
    getElements(name?: string): Element[];
  }
}

/**
 * Mount more properties on the prototype of the `JSDOM`.
 * __WARNING__ This is dangerous with some side effects due to it changes the prototype of DOM.
 */
export function mountDomPrototype(): void {
  const { window: g } = new JSDOM();

  if (g.Element.prototype.hasMounted) {
    return;
  }

  Object.defineProperty(g.Element.prototype, 'hasMounted', {
    configurable: false,
    enumerable: false,
    writable: false,
    value: true,
  });

  Object.defineProperty(g.Element.prototype, 'elementValue', {
    configurable: false,
    enumerable: false,
    get(this: Element) {
      let textNode: Node | undefined;
      Array.from(this.childNodes).forEach(node => {
        if (node.nodeType === this.TEXT_NODE) {
          textNode = node;
        }
      });
      if (textNode && textNode.nodeValue) {
        return textNode.nodeValue;
      }
      return '';
    },
    set(this: Element, value: string) {
      let textNode: Node | undefined;
      Array.from(this.childNodes).forEach(node => {
        if (node.nodeType === this.TEXT_NODE) {
          textNode = node;
        }
      });
      this.removeAllChildNodes();
      if (textNode) {
        textNode.nodeValue = value;
      } else {
        textNode = (this.ownerDocument as Document).createTextNode(value);
      }
      this.appendChild(textNode);
    },
  });

  Object.defineProperty(g.Element.prototype, 'removeAllAttributes', {
    configurable: false,
    enumerable: false,
    writable: false,
    value(this: Element): Element {
      while (this.attributes.length > 0) {
        this.removeAttribute(this.attributes[0].name);
      }
      return this;
    },
  });

  Object.defineProperty(g.Element.prototype, 'appendChildClone', {
    configurable: false,
    enumerable: false,
    writable: false,
    value<T extends Node>(this: Element, newChild: T): T {
      const clone =
        this.ownerDocument === newChild.ownerDocument
          ? (newChild.cloneNode(true) as T)
          : (this.ownerDocument as Document).importNode(newChild, true);
      this.appendChild(clone);
      return clone;
    },
  });

  Object.defineProperty(g.Element.prototype, 'appendChildren', {
    configurable: false,
    enumerable: false,
    writable: false,
    value<T extends Node>(
      this: Element,
      newChildren: Iterable<T> | ArrayLike<T>,
    ): Element {
      Array.from(newChildren).forEach(newChild => this.appendChild(newChild));
      return this;
    },
  });

  Object.defineProperty(g.Element.prototype, 'appendChildrenClone', {
    configurable: false,
    enumerable: false,
    writable: false,
    value<T extends Node>(
      this: Element,
      newChildren: Iterable<T> | ArrayLike<T>,
    ): Element {
      Array.from(newChildren).forEach(newChild => this.appendChildClone(newChild));
      return this;
    },
  });

  Object.defineProperty(g.Element.prototype, 'removeAllChildNodes', {
    configurable: false,
    enumerable: false,
    writable: false,
    value(this: Element): Element {
      Array.from(this.childNodes).forEach(child => this.removeChild(child));
      return this;
    },
  });

  Object.defineProperty(g.Element.prototype, 'getElement', {
    configurable: false,
    enumerable: false,
    writable: false,
    value(this: Element, name?: string): Element | undefined {
      return name
        ? Array.from(this.children).find(child => child.tagName === name)
        : this.children[0];
    },
  });

  Object.defineProperty(g.Element.prototype, 'getElements', {
    configurable: false,
    enumerable: false,
    writable: false,
    value(this: Element, name?: string): Element[] {
      return name
        ? Array.from(this.children).filter(child => child.tagName === name)
        : Array.from(this.children);
    },
  });
}
