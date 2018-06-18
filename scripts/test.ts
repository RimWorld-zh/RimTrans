/**
 * Test
 */

import fs from 'fs';
import xmljs from 'xml-js';

const doc: xmljs.Element = xmljs.xml2js(fs.readFileSync('test.xml', 'utf-8'), {
  compact: false,
  alwaysChildren: true,
}) as xmljs.Element;

console.log(JSON.stringify(doc, undefined, '  '));
