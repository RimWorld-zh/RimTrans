/* eslint-disable @typescript-eslint/no-explicit-any */

export function prop(
  namespace: string,
  props: Record<string, any>,
): Record<string, boolean> {
  return Object.fromEntries(
    Object.entries(props).map(([name, value]) => [
      `${namespace}p-${name}_${value}`,
      value !== null && value !== undefined,
    ]),
  );
}

export function when(states: Record<string, any>): Record<string, boolean> {
  return Object.fromEntries(
    Object.entries(states).map(([name, value]) => [`is-${name}`, !!value]),
  );
}
