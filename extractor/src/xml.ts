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
