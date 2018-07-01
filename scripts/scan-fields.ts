/**
 * Scan fields label and description in all Defs.
 */

/**
 * Test definition.ts
 */

import fs from 'fs';
import globby from 'globby';
import { readRawContents } from './utils';
import init from './env-init';
import * as xml from '../core/xml';
import * as definition from '../core/definition';
import { Dictionary } from '../common/collection';
import Stack from '../common/stack';

const { dirCore } = init();

async function scan(): Promise<void> {
  const rawContents: Dictionary<string> = await readRawContents(
    `${dirCore}/Defs/**/*.xml`,
  );

  const fields: Set<string> = new Set();
  const stack: Stack<string> = new Stack();

  // tslint:disable-next-line:typedef
  const scanElementRecursively = (element: xml.Node) => {
    if (!xml.isElement(element)) {
      return;
    }
    stack.push(element.name);
    if (element.value) {
      const text: string = element.value;
      if (text.includes(' ') || text.includes(',') || text.endsWith('.')) {
        fields.add(stack.items.join('.'));
      }
    } else {
      element.nodes.forEach(scanElementRecursively);
    }
    stack.pop();
  };

  Object.entries(rawContents).forEach(([path, content]) => {
    const defs: xml.Element = xml.parse(content);
    defs.nodes.forEach(scanElementRecursively);
  });

  fs.writeFile(
    'scripts/fields.txt',
    Array.from(fields)
      .sort()
      .join('\n'),
    error => {
      if (error) {
        throw error;
      }
    },
  );
}

scan().catch(error => console.log(error));
