const parser = new DOMParser();

/**
 * Parse xml text to XML DOM
 * @param xml The text content of the xml
 */
export function parse(xml: string): XMLDocument {
  return parser.parseFromString(xml, 'text/xml');
}
