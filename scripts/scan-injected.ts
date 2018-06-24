/**
 * Scan all injections in RimWorld languages data.
 */

import dotenv from 'dotenv';
import fs from 'fs';
import envInit from './env-init';
import readFiles from './read-files';

import * as xml from '../core/xml';
import { schema } from '../core/schema';
import { stringCompare } from '../core/utils';

const { dirCore } = envInit();

function scanInjected(): void {
  // tslint:disable:no-any
  const data: any = {};

  function addData(defType: string, fieldPath: string): void {
    if (!data[defType]) {
      data[defType] = {};
    }

    fieldPath
      .replace(/\.\d+/g, '.li')
      .split('.')
      .slice(1)
      .reduce((graph, field) => {
        if (!graph[field]) {
          graph[field] = {};
        }

        return graph[field];
      }, data[defType]);
  }

  readFiles(`${dirCore}/Languages/ChineseSimplified/DefInjected/**/*.xml`).then(
    rawContents => {
      Object.entries(rawContents).forEach(([path, content]) => {
        const pathNodes: string[] = path.split('/');
        const defType: string = pathNodes[pathNodes.length - 2];
        const root: xml.Element | undefined = xml.parse(content, path);
        if (root) {
          root.nodes.filter(xml.isElement).forEach(def => addData(defType, def.name));
        }
      });
      Object.entries(data)
        .sort((a, b) => stringCompare(a[0], b[0]))
        .forEach(([defType, definition]) => {
          console.log(`"${defType}": ${JSON.stringify(definition, undefined, '  ')},`);
        });
    },
  );
}

scanInjected();
