import * as io from '@rimtrans/io';

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

export interface TypePackage {
  readonly classes: readonly ClassInfo[];
  readonly enums: readonly EnumInfo[];
}

export async function load(paths: string[]): Promise<Record<string, ClassInfo>> {
  const result: Record<string, ClassInfo> = {};

  const typePackages = await Promise.all(paths.map(path => io.load<TypePackage>(path)));

  return result;
}
