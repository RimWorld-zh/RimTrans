import pth from 'path';
import { pathExists } from 'fs-extra';
import globby, { GlobbyOptions } from 'globby';

export * from 'fs-extra';

/**
 * Combine parts of path.
 * @param paths the parts of expected path
 */
export function join(...paths: string[]): string {
  return pth.join(...paths);
}

/**
 * Get the directory of the file.
 * @param path the path
 */
export function directoryName(path: string): string {
  return pth.dirname(path);
}

/**
 * Get the file name in the path
 * @param path the path
 * @param removeExt remove extension name or not, default `true`
 */
export function fileName(path: string, removeExt: boolean = true): string {
  const ext = removeExt ? pth.extname(path) : undefined;
  return pth.basename(path, ext);
}

/**
 * Get the extension name of the path
 * @param path the path
 */
export function extensionName(path: string): string {
  return pth.extname(path);
}

/**
 * Search files or directories by glob patterns, based on `globby`.
 * @param patterns the glob patterns
 * @param options the globby options
 */
export async function search(
  patterns: string[],
  options: GlobbyOptions = {},
): Promise<string[]> {
  if (options.cwd && !(await pathExists(options.cwd))) {
    return [];
  }

  // Try multiple times for ensuring corrected result
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
