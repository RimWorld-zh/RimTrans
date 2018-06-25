export interface RawContents {
  [path: string]: string;
}

/**
 * String compare function.
 * @param ignoreCase ignore upper and lower case or not
 */
export function stringCompare(a: string, b: string, ignoreCase: boolean = false): number {
  const A: string = a.toUpperCase();
  const B: string = b.toUpperCase();

  if (A < B) {
    return -1;
  }
  if (A > B) {
    return 1;
  }

  return 0;
}

export function arrayInsert<T>(array: T[], index: number, ...items: T[]): void {
  array.splice(index, 0, ...items);
}
export function arrayInsertAfter<T>(array: T[], index: number, ...items: T[]): void {
  array.splice(index + 1, 0, ...items);
}
