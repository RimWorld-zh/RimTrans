/**
 * Dictionary mode hash map for object type.
 */
export interface Dictionary<T> {
  [key: string]: T;
}

/**
 * Simple key value pair structure.
 */
export interface KeyValuePair<TKey, TValue> {
  key: TKey;
  value: TValue;
}

/**
 * Insert items into the specific index of an array.
 * @param array the array
 * @param index target position
 * @param items items to insert
 */
export function arrayInsert<T>(array: T[], index: number, ...items: T[]): void {
  array.splice(index, 0, ...items);
}

/**
 * Insert items after the specific index of an array.
 * @param array the array
 * @param index target position
 * @param items items to insert
 */
export function arrayInsertAfter<T>(array: T[], index: number, ...items: T[]): void {
  array.splice(index + 1, 0, ...items);
}
