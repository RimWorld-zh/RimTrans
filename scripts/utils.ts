import fs from 'fs';
import globby from 'globby';
import { Dictionary } from '../common/collection';

/**
 * Read all files as `RawContents` of matching paths.
 */
export async function readRawContents(
  patterns: string | string[],
): Promise<Dictionary<string>> {
  const defFiles: string[] = await globby(patterns);
  // const defFiles: string[] = ['temp.xml'];

  return new Promise<Dictionary<string>>((resolve, reject) => {
    const rawContents: Dictionary<string> = {};
    let count: number = 0;

    defFiles.forEach(path =>
      fs.readFile(path, { encoding: 'utf-8' }, (error, content) => {
        if (error) {
          reject(error);
        }
        rawContents[path] = content;
        count++;
        if (count === defFiles.length) {
          resolve(rawContents);
        }
      }),
    );
  });
}
