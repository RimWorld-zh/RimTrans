import fs from 'fs';
import pth from 'path';

export interface ClassInfo {
  readonly isAbstract: boolean;
  readonly baseClass: string;
  readonly name: string;
  readonly fields: readonly FieldInfo[];
}

export interface EnumInfo {
  readonly name: string;
  readonly values: readonly EnumValue[];
}

export interface EnumValue {
  readonly name: string;
  readonly value: number;
}

export interface FieldInfo {
  readonly name: string;
  readonly mustTranslate: boolean;
  readonly type: TypeInfo;
}

export type TypeCategory =
  | 'DEF'
  | 'TYPE'
  | 'VALUE'
  | 'CLASS'
  | 'ENUM'
  | 'LIST'
  | 'DICT'
  | 'UNKNOWN';

export interface TypeInfo {
  readonly category: TypeCategory;
  readonly name: string;
  readonly of: TypeInfo | null;
}

export interface TypeInfoMap {
  readonly classes: readonly ClassInfo[];
  readonly enums: readonly EnumInfo[];
}

export async function getCoreTypeInfo(): Promise<TypeInfoMap> {
  const content = await fs.promises.readFile(
    pth.join(__dirname, '..', 'type-info.json'),
    'utf-8',
  );
  return JSON.parse(content);
}
