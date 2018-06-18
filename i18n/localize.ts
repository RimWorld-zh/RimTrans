// tslint:disable:no-any
/**
 *
 */
export default function localize(key: string, ...args: any[]): string {
  let result: string = key;

  args.forEach((a, i) => (result = result.replace(RegExp(`\{${i}\}`), a)));

  return result;
}
