import pth from 'path';
import fse from 'fs-extra';
import globby from 'globby';

import { ATTRIBUTE_MUST_TRANSLATE } from './constants';

import { ExtractorEventEmitter } from './extractor-event-emitter';

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

export interface TypeMaps {
  readonly classInfoMap: Readonly<Record<string, ClassInfo>>;
  readonly enumInfoMap: Readonly<Record<string, EnumInfo>>;
}

export class TypePackageExtractor {
  /* eslint-disable lines-between-class-members */
  public readonly ACTION_LOAD = 'Assemblies Load';
  /* eslint-enable lines-between-class-members */

  private readonly emitter: ExtractorEventEmitter;

  public constructor(emitter: ExtractorEventEmitter) {
    this.emitter = emitter;
  }

  /**
   * Load `TypePackage` json files or Assemblies directories
   * @param paths the path array of files and directories, `[Core, ...Mods]`.
   */
  public async load(paths: string[]): Promise<TypeMaps> {
    const action = this.ACTION_LOAD;

    const typePackages = await Promise.all(
      paths.map(
        async (path): Promise<TypePackage> => {
          if (await fse.pathExists(path)) {
            if (
              await fse
                .lstat(path)
                .then(stats => stats.isFile())
                .catch(() => false)
            ) {
              const pkg = await fse.readJSON(path);
              return pkg;
            }
            return { classes: [], enums: [] };
          }
          return { classes: [], enums: [] };
        },
      ),
    );

    const maps = this.mergeTypePackages(typePackages);

    return maps;
  }

  private async mergeTypePackages(typePackages: TypePackage[]): Promise<TypeMaps> {
    const classInfoMap: Record<string, ClassInfo> = {};
    const enumInfoMap: Record<string, EnumInfo> = {};

    // Deduplicate
    typePackages.forEach(pack => {
      if (pack.classes) {
        pack.classes.forEach(classInfo => {
          if (!classInfoMap[classInfo.name]) {
            classInfoMap[classInfo.name] = classInfo;
          }
        });
      }
      if (pack.enums) {
        pack.enums.forEach(enumInfo => {
          if (!enumInfoMap[enumInfo.name]) {
            enumInfoMap[enumInfo.name] = enumInfo;
          }
        });
      }
    });

    // Inheritance
    const allClasses = Object.values(classInfoMap);
    const resolveInherit = (classInfo: ClassInfo): void => {
      const children = allClasses.filter(ci => ci.baseClass === classInfo.name);
      children.forEach(child => {
        (child.fields as FieldInfo[]).unshift(...classInfo.fields);
        resolveInherit(child);
      });
    };
    allClasses
      .filter(classInfo => !classInfo.baseClass || !classInfoMap[classInfo.baseClass])
      .forEach(classInfo => resolveInherit(classInfo));

    // Fix
    typePackages.forEach(
      pack =>
        pack.fix &&
        Object.entries(pack.fix).forEach(([className, fieldNames]) => {
          const classInfo: ClassInfo | undefined = classInfoMap[className];
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

    return { classInfoMap, enumInfoMap };
  }
}
