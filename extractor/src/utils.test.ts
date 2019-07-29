import { genPathResolve } from '@huiji/shared-utils';

export const resolvePath = genPathResolve(__dirname, '..', '..');

export const defsFileCount = 413;

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

export const pathEnglishKeyed = resolvePath('Core', 'Languages', 'English', 'Keyed');

export const pathsKeyed = [
  resolvePath('Core', 'Languages', 'Template', 'Keyed'),
  resolvePath('Core', 'Languages', 'Mock', 'Keyed'),
];

export const pathEnglishStrings = resolvePath('Core', 'Languages', 'English', 'Strings');

export const pathsStrings = [
  resolvePath('Core', 'Languages', 'Template', 'Strings'),
  resolvePath('Core', 'Languages', 'Mock', 'Strings'),
];

// output

export const TEMP = '.tmp';

export const pathTemp = resolvePath(TEMP);

export const outputMods = resolvePath(TEMP, 'mods.json');

export const outputInheritedDefs = resolvePath(TEMP, 'InheritedDefs');

export const outputInjectionMapLoaded = resolvePath(TEMP, 'injection-maps-loaded.json');
export const outputInjectionMapParsed = resolvePath(TEMP, 'injection-maps-parsed.json');
export const outputInjectionMapParsedFuzzy = resolvePath(
  TEMP,
  'injection-maps-parsed-fuzzy.json',
);
export const outputInjectionMapMerged = resolvePath(TEMP, 'injection-maps-merged.json');

export const outputMissing = resolvePath(TEMP, 'missing.txt');
export const outputFuzzy = resolvePath(TEMP, 'fuzzy.txt');

export const outputDefInjected = resolvePath(TEMP, 'DefInjected');
export const outputDefInjectedFuzzy = resolvePath(TEMP, 'DefInjectedFuzzy');

export const outputKeyedEnglish = resolvePath(TEMP, 'keyed-map-english.json');
export const outputKeyedOld = resolvePath(TEMP, 'keyed-map-old.json');
export const outputKeyedNew = resolvePath(TEMP, 'keyed-map-new.json');

export const outputKeyed = resolvePath(TEMP, 'Keyed');

export const outputStrings = resolvePath(TEMP, 'Strings');

export const outputExtractor = resolvePath(TEMP, 'Mods');
