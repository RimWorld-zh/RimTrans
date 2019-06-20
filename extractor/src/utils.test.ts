import { genPathResolve } from '@huiji/shared-utils';
import * as xml from './xml';

xml.mountDomPrototype();

export const TEMP = '.tmp';

export const defsFileCount = 413;

export const resolvePath = genPathResolve(__dirname, '..', '..');

export const pathTestMods = resolvePath('.tmp.mods');

export const pathCore = resolvePath('Core');

export const pathsDefs = [resolvePath('Core', 'Defs'), resolvePath('Mock', 'Defs')];

export const pathsTypePackage = [
  resolvePath('Reflection', 'type-package.json'),
  resolvePath('Reflection', 'type-package-fix.json'),
];

export const pathsDefInjected = [
  resolvePath('Core', 'Languages', 'Template', 'DefInjected'),
  resolvePath('Core', 'Languages', 'Mock', 'DefInjected'),
];

export const pathTemp = resolvePath(TEMP);

export const outputMods = resolvePath(TEMP, 'mods.json');

export const outputInheritedDefs = resolvePath(TEMP, 'InheritedDefs');

export const outputInjectionMapLoaded = resolvePath(TEMP, 'injection-maps-loaded.json');
export const outputInjectionMapParsed = resolvePath(TEMP, 'injection-maps-parsed.json');
export const outputInjectionMapMerged = resolvePath(TEMP, 'injection-maps-merged.json');

export const outputMissing = resolvePath(TEMP, 'missing.txt');
export const outputFuzzy = resolvePath(TEMP, 'fuzzy.txt');

export const outputDefInjected = resolvePath(TEMP, 'DefInjected');
export const outputDefInjectedFuzzy = resolvePath(TEMP, 'DefInjectedFuzzy');

export const outputExtractor = resolvePath(TEMP, 'Mods');
export const outputBenchmark = resolvePath(TEMP, 'benchmark.txt');
