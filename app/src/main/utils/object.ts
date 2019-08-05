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

export function clone<T>(obj: T): T {
  return JSON.parse(JSON.stringify(obj));
}
