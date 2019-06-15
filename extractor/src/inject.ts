import * as io from '@rimtrans/io';
import * as xml from './xml';
import { TypePackage, ClassInfo } from './type-info';

xml.mountDomPrototype();

interface DefInjected {
  /**
   * The Def type of the injected, such as 'BiomeDef', 'DamageDef'
   */
  defType: string;

  filename: string;

  defName: string;

  fields: DefInjectedField[];
}

interface DefInjectedField {
  name: string;

  /**
   * The origin English text value.
   */
  origin: string | string[];

  /**
   * The translation text value.
   */
  translation: string | string[];

  fields: DefInjectedField[];
}

export function parse(
  defMaps: Record<string, Element[]>[],
  typeInfoMaps: TypePackage[],
): void {
  //
}

/**
 * Resolve type info, `[current, dependencies, core]`
 * @param typePackages the array of `TypePackage`
 */
export function resolveTypeInfo(typePackages: TypePackage[]): Record<string, ClassInfo> {
  const result: Record<string, ClassInfo> = {};

  typePackages.forEach(pack => pack.classes.forEach(ci => (result[ci.name] = ci)));

  const allClassInfos = Object.values(result);

  return result;
}
