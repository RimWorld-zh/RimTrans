/* eslint-disable @typescript-eslint/no-explicit-any */

export type SortDirection = 'ASC' | 'DESC';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function sort<T extends any>(
  list: T[],
  properties: (keyof T)[],
  direction: SortDirection,
): T[] {
  const signal = (direction === 'ASC' && -1) || 1;
  return list.sort((a, b) => {
    for (const p of properties) {
      const va = a[p];
      const vb = b[p];
      let delta: number;
      if (typeof va === 'number' && typeof vb === 'number') {
        delta = (va - vb) * signal;
      } else if (typeof va === 'string' && typeof vb === 'string') {
        delta = va.localeCompare(vb) * signal;
      } else {
        delta = `${va}`.localeCompare(`${vb}`) * signal;
      }
      if (delta !== 0) {
        return delta;
      }
    }

    return 0;
  });
}

/**
 * Clone an object by `JSON.stringify` and JSON.parse
 * @param obj the object to be cloned
 */
export function cloneObject<T>(obj: T): T {
  return JSON.parse(JSON.stringify(obj));
}

/**
 * Deep compare two object.
 * @param a the first object
 * @param b the second object
 */
export function objectEqual(a: any, b: any): boolean {
  const tA = typeof a;
  const tB = typeof b;
  if (tA !== tB) {
    return false;
  }

  if (Object.is(a, b)) {
    return true;
  }
  if (a === null || b === null) {
    return a === b;
  }

  switch (tA) {
    case 'bigint':
    case 'boolean':
    case 'string':
    case 'symbol':
    case 'undefined':
      return a === b;

    case 'number':
      if (Number.isNaN(a as number) && Number.isNaN(b as number)) {
        return true;
      }
      return a === b;

    case 'function':
      return true;

    default:
  }

  const aIsArray = Array.isArray(a);
  const bIsArray = Array.isArray(b);

  if ((aIsArray && !bIsArray) || (!aIsArray && bIsArray)) {
    return false;
  }

  // Array
  if (aIsArray && bIsArray) {
    if (a.length !== b.length) {
      return false;
    }
    const { length } = a;
    for (let i = 0; i < length; i++) {
      const vA = a[i];
      const vB = b[i];
      if (!objectEqual(vA, vB)) {
        return false;
      }
    }
    return true;
  }

  // Object;
  const keys = [...new Set([...Object.keys(a), ...Object.keys(b)])];
  for (const k of keys) {
    const vA = a[k];
    const vB = b[k];
    if (!objectEqual(vA, vB)) {
      return false;
    }
  }
  return true;
}
