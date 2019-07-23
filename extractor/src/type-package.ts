import * as io from '@rimtrans/io';
import { ATTRIBUTE_MUST_TRANSLATE } from './constants';

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

export class TypePackage {
  /**
   *
   * @param paths the array of paths to `TypePackage` json files, `[Core, ...Mods]`.
   */
  public static async load(paths: string[]): Promise<Record<string, ClassInfo>> {
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
}
