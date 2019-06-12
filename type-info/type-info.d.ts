/* eslint-disable no-redeclare */

declare const RimTransTypeInfo: RimTransTypeInfo.TypeInfoMap;

declare namespace RimTransTypeInfo {
  interface TypeInfoMap {
    readonly classes: readonly ClassInfo[];
    readonly enums: readonly EnumInfo[];
  }

  interface ClassInfo {
    readonly isAbstract: boolean;
    readonly baseClass: string;
    readonly name: string;
    readonly fields: readonly FieldInfo[];
  }

  interface EnumInfo {
    readonly name: string;
    readonly values: readonly EnumValue[];
  }

  interface EnumValue {
    readonly name: string;
    readonly value: number;
  }

  interface FieldInfo {
    readonly name: string;
    readonly mustTranslate: boolean;
    readonly type: TypeInfo;
  }

  type TypeCategory =
    | 'DEF'
    | 'TYPE'
    | 'VALUE'
    | 'CLASS'
    | 'ENUM'
    | 'LIST'
    | 'DICT'
    | 'UNKNOWN';

  interface TypeInfo {
    readonly category: TypeCategory;
    readonly name: string;
    readonly of: TypeInfo | null;
  }
}

export = RimTransTypeInfo;
