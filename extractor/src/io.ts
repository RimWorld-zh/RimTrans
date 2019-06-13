import fs from 'fs';
import pth from 'path';
import globby, { GlobbyOptions } from 'globby';

export function getFiles(patterns: string[], options?: GlobbyOptions): Promise<string[]> {
  return Promise.all(
    Array(3)
      .fill(0)
      .map(() => globby(patterns, options)),
  ).then(agg => [
    ...new Set(
      agg.reduce<string[]>((result, cur) => {
        result.push(...cur);
        return result;
      }, []),
    ),
  ]);
}
