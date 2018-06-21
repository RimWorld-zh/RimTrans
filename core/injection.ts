/**
 * Injection
 */

import * as logger from './logger';
import * as xml from './xml';
import { DefinitionData, RawContents } from './definition';
import { stringCompare } from './utils';

/**
 * The default end of line character. Use \n for LF and \r\n for CRLF.
 * 默认行尾字符。使用 \n 表示 LF，\r\n 表示 CRLF。
 */
export enum EOL {
  LF = '\n',
  CRLF = '\r\n',
}

export interface Field {
  name: string;
  text?: string;
  children: Field[];
}

export interface Injection {
  commentBefore?: string;
  conflicted: boolean;
  defName: string;
  fields: Field[];
}

export interface InjectionData {
  [defType: string]: {
    [fileName: string]: {
      injections: Injection[];
      comments: string[];
    };
  };
}

/**
 * Parse the definition data and extract injection data.
 */
export function parse(rawContents: RawContents): void {
  const data: InjectionData = {};
  // tslint:disable-next-line:typedef
  const initFile = (defType: string, fileName: string): void => {
    if (!data[defType]) {
      data[defType] = {};
    }
    if (!data[defType][fileName]) {
      data[defType][fileName] = {
        injections: [],
        comments: [],
      };
    }
  };
  // tslint:disable-next-line:typedef
  const addInjection = (defType: string, fileName: string, inj: Injection): void => {
    initFile(defType, fileName);
    data[defType][fileName].injections.push(inj);
  };
  // tslint:disable-next-line:typedef
  const addComment = (defType: string, fileName: string, comment: string): void => {
    initFile(defType, fileName);
    data[defType][fileName].comments.push(comment);
  };

  Object.entries(rawContents)
    .sort((a, b) => stringCompare(a[0], b[0]))
    .forEach(([path, raw]) => {
      const pathNodes: string[] = path.split(/\\|\//);
      const defType: string = pathNodes[pathNodes.length - 2];
      const fileName: string = pathNodes[pathNodes.length - 1];
      if (pathNodes.length < 2 || !defType || !fileName) {
        logger.error(`Invalid path "${path}" for DefInjected`);

        return;
      }

      const root: xml.Element | undefined = xml.parse(raw, path);
      if (!root) {
        return;
      }

      root.nodes.forEach(node => {
        if (node.type === 'comment') {
          addComment(defType, fileName, (node as xml.Comment).comment);
        }
      });
    });
}
