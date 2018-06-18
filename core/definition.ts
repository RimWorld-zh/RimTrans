/**
 * Definition
 */

import * as logger from './logger';
import * as xml from './xml';
import { stringCompare } from './utils';

export interface DefinitionData {
  [defType: string]: xml.Element[];
}

/**
 * Parse the XML documents to RimWorld Defs data.
 * @param rawDefsContent key for file path, value for XML plain text.
 */
export function parse(rawDefsContent: { [path: string]: string }): DefinitionData {
  const data: DefinitionData = {};
  const addDefinition: (def: xml.Element) => void = def => {
    if (!data[def.name]) {
      data[def.name] = [];
    }
    data[def.name].push(def);
  };

  Object.entries(rawDefsContent)
    .sort((a, b) => stringCompare(a[0], b[0]))
    .forEach(([path, raw]) => {
      let root: xml.Element | undefined;
      try {
        root = xml.parse(raw);
      } catch (error) {
        logger.error(error);

        return;
      }

      if (!root || !root.nodes) {
        logger.error(
          `Missing root element "Defs" or the Defs has no elements.\nfile: "${path}".`,
        );

        return;
      }

      root.nodes.reduce((pre, cur) => {
        const comment: string | undefined = validComment(pre);
        const def: xml.Element | undefined = xml.asElement(cur);

        if (def) {
          if (!isAbstract(def)) {
            if (comment) {
              def.attributes.CommentBefore = comment;
            }
            def.attributes.FilePath = path;
            addDefinition(def);
          }
        }

        return cur;
      });
    });

  return data;
}

function validComment(node: xml.Node): string | undefined {
  const comment: xml.Comment | undefined = xml.asComment(node);

  return comment &&
    comment.comment &&
    (comment.comment.match(/\r?\n/g) || []).length === 0
    ? comment.comment
    : undefined;
}

function isAbstract(def: xml.Element): boolean {
  return !!def.attributes && def.attributes.Abstract !== 'True';
}
