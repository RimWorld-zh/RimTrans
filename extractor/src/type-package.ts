import * as io from '@rimtrans/io';

// Learn more in the project 'Reflection'

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
  readonly attributes: string[];
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
  readonly of?: TypeInfo;
}

export interface TypePackage {
  readonly classes: readonly ClassInfo[];
  readonly enums: readonly EnumInfo[];
}

/**
 *
 * @param paths the array of paths to `TypePackage` json files, `[core, ...mods]`.
 */
export async function load(paths: string[]): Promise<Record<string, ClassInfo>> {
  const map: Record<string, ClassInfo> = {};

  const typePackages = await Promise.all(
    paths.map(path =>
      io
        .fileExists(path)
        .then<TypePackage>(exists =>
          exists ? io.load<TypePackage>(path) : { classes: [], enums: [] },
        ),
    ),
  );

  typePackages.forEach(pack =>
    pack.classes.forEach(classInfo => {
      if (!map[classInfo.name]) {
        map[classInfo.name] = classInfo;
      }
    }),
  );

  const allClasses = Object.values(map);
  const resolveInherit = (classInfo: ClassInfo): void => {
    const children = allClasses.filter(ci => ci.baseClass === classInfo.name);
    children.forEach(child => {
      (child.fields as FieldInfo[]).unshift(...classInfo.fields);
      resolveInherit(child);
    });
  };
  allClasses
    .filter(classInfo => !map[classInfo.baseClass])
    .forEach(classInfo => resolveInherit(classInfo));

  return map;
}

// Constants
export const ATTRIBUTE_UNSAVED = 'Unsaved';
export const ATTRIBUTE_NO_TRANSLATE = 'NoTranslate';
export const ATTRIBUTE_MUST_TRANSLATE = 'MustTranslate';
export const ATTRIBUTE_MAY_TRANSLATE = 'MayTranslate';
export const ATTRIBUTE_TRANSLATION_CAN_CHANGE_COUNT = 'TranslationCanChangeCount';
export const TYPE_STRING = 'String';
