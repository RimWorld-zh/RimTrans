import fs from 'fs';
import globby from 'globby';
import * as xml from '../core/xml';

/**
 * Read files by glob.
 */
export default function readFiles(patterns: string | string[]): Promise<xml.RawContents> {
  return new Promise<xml.RawContents>(async (resolve, reject) => {
    const rawContents: xml.RawContents = {};

    const files: string[] = await globby(patterns);
    let count: number = 0;

    files.forEach(f =>
      fs.readFile(f, { encoding: 'utf-8' }, (error, data) => {
        if (error) {
          reject(error);
        }
        rawContents[f] = data;
        count++;
        if (count === files.length) {
          resolve(rawContents);
        }
      }),
    );
  });
}
