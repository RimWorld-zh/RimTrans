import * as io from '@rimtrans/io';

// Learn more in the project 'Reflection'

export interface ClassInfo {
  readonly isAbstract: boolean;
  readonly baseClass?: string;
  readonly name: string;
  readonly fields: readonly FieldInfo[];
  readonly handles: readonly HandleInfo[];
}

export interface HandleInfo {
  readonly field: string;
  readonly priority: number;
  readonly value: string;
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
  readonly alias?: string;
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
  readonly classes?: readonly ClassInfo[];
  readonly enums?: readonly EnumInfo[];
  readonly fix?: Record<string, string[]>;
}

// Constants
export const ATTRIBUTE_UNSAVED = 'Unsaved';
export const ATTRIBUTE_NO_TRANSLATE = 'NoTranslate';
export const ATTRIBUTE_MUST_TRANSLATE = 'MustTranslate';
export const ATTRIBUTE_MAY_TRANSLATE = 'MayTranslate';
export const ATTRIBUTE_LOAD_ALIAS = 'LoadAlias';
export const ATTRIBUTE_TRANSLATION_CAN_CHANGE_COUNT = 'TranslationCanChangeCount';
export const TYPE_STRING = 'String';

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

  typePackages.forEach(
    pack =>
      pack.classes &&
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
    .filter(classInfo => !classInfo.baseClass || !map[classInfo.baseClass])
    .forEach(classInfo => resolveInherit(classInfo));

  typePackages.forEach(
    pack =>
      pack.fix &&
      Object.entries(pack.fix).forEach(([className, fieldNames]) => {
        const classInfo: ClassInfo | undefined = map[className];
        if (classInfo) {
          fieldNames.forEach(fieldName => {
            const fieldInfo: FieldInfo | undefined = classInfo.fields.find(
              fi => fi.name === fieldName,
            );
            if (fieldInfo && !fieldInfo.attributes.includes(ATTRIBUTE_MUST_TRANSLATE)) {
              fieldInfo.attributes.push(ATTRIBUTE_MUST_TRANSLATE);
            }
          });
        }
      }),
  );

  return map;
}
