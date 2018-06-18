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
