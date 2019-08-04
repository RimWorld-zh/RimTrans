/* eslint-disable @typescript-eslint/no-explicit-any */
import { English } from '../src/languages/english';

export function genTypes(): string {
  const parse = (dict: any): any => {
    const current: any = {};
    Object.entries(dict).forEach(([k, v]) => {
      current[k] = (typeof v === 'string' && 'string') || parse(v);
    });
    return current;
  };

  const root = parse(English.dict);

  const block = (record: any, depth: number, partial: boolean): string => {
    // indent
    const i = '  '.repeat(depth);
    // question mark
    const q = (partial && '?') || '';

    const rows = Object.entries(record).map(([k, v]) => {
      // type
      const t =
        (v === 'string' && 'string') || `{\n${block(v, depth + 1, partial)}\n${i}}`;

      return `${i}readonly ${k}${q}: ${t};`;
    });

    return rows.join('\n');
  };

  const generate = (name: string, record: any, partial: boolean): string => {
    const b = block(record, 1, partial);
    return `export interface ${name} {\n${b}\n}\n`;
  };

  return [
    '// generate by script/gen-types.ts, do not edit this file by hand.',
    generate('TranslationDictionary', root, false),
    generate('TranslationDictionaryPartial', root, true),
  ].join('\n');
}
